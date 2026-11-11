using System.Text.RegularExpressions;
using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Sap;
using Mignow.Atc.Application.Ports.Sap.Ddic;
using Mignow.Atc.Application.Ports.Time;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Modules.Remediation.Apply;

/// <summary>
/// Simulação, aplicação e rollback no SAP (porte de <c>aplicar_adt.ps1</c>, <c>aplicar_ddic.ps1</c> e <c>cmd_aplicacao</c>). Objeto por objeto, EM FILA
/// (nunca em paralelo), com a request da sessão, sempre pelo <see cref="ISapWriter"/>/<see cref="IDdicWriter"/> (trava de produção).
/// Antes de gravar: a versão ativa precisa ser igual ao fonte lido (senão <c>ABORTADO_FONTE_MUDOU</c>); o backup é o fonte ativo; o fonte novo é
/// montado de novo a partir dos blocos da proposta sobre o fonte lido (assim itens do mesmo fonte se aplicam em sequência, como o
/// <c>preparar</c> depois de um <c>ATIVADO</c>). Estrutura/tabela do dicionário: <see cref="DdicApplyRules"/>.
/// </summary>
public sealed class ApplyEngine(ISapReader reader, ISapWriter writer, IDdicWriter ddicWriter, IDateTimeProvider clock, IStringLocalizer<Errors> l)
{
    public const string ModeSimulation = "simulacao";
    public const string ModeApply = "aplicacao";
    public const string ModeRollback = "rollback";

    // ---------------------------------------------------------------- simulação

    public async Task<ApplyBatch> SimulateAsync(ApplyInput input, EngineHost host)
    {
        List<ApplyItemReport> reports = [];
        var done = 0;
        foreach (var item in input.Items)
        {
            if (await host.IsCancelledAsync())
                break;
            reports.Add(await RunItemAsync(input, host, item, ModeSimulation));
            await host.ProgressAsync(++done, input.Items.Count);
        }
        await host.SaveAsync();
        return new ApplyBatch(reports, null, null);
    }

    // ---------------------------------------------------------------- aplicação

    public async Task<ApplyBatch> ApplyAsync(ApplyInput input, EngineHost host)
    {
        List<ApplyItemReport> reports = [];
        if (string.IsNullOrWhiteSpace(input.Request))
            return new ApplyBatch(reports, "Apply.RequestRequired", l.Text("Apply.RequestRequired"));
        var done = 0;
        foreach (var item in input.Items.OrderBy(i => i.ObjectName, StringComparer.Ordinal).ThenBy(i => i.N))
        {
            if (await host.IsCancelledAsync())
                break;
            var report = await RunItemAsync(input, host, item, ModeApply);
            reports.Add(report);
            await host.ProgressAsync(++done, input.Items.Count);
            await host.SaveAsync();
            if (report.Result == "ERRO_BLOQUEIO")
                return new ApplyBatch(reports, report.Detail, report.Detail);
        }
        return new ApplyBatch(reports, null, null);
    }

    // ---------------------------------------------------------------- rollback

    public async Task<ApplyBatch> RollbackAsync(ApplyInput input, EngineHost host)
    {
        List<ApplyItemReport> reports = [];
        foreach (var item in input.Items)
        {
            var report = await RunItemAsync(input, host, item, ModeRollback);
            reports.Add(report);
            await host.SaveAsync();
            if (report.Result == "ERRO_BLOQUEIO")
                return new ApplyBatch(reports, report.Detail, report.Detail);
        }
        return new ApplyBatch(reports, null, null);
    }

    // ---------------------------------------------------------------- um item

    private async Task<ApplyItemReport> RunItemAsync(ApplyInput input, EngineHost host, RemediationItem item, string mode)
    {
        var p = item.Proposal;
        var prep = item.Preparation;
        if (mode != ModeRollback)
        {
            var confirmed = item.Status is RemediationStatus.ConfirmedKb or RemediationStatus.ConfirmedAi;
            if (p is not { Action: ItemProposal.ActionFix } || prep?.NewSource is null || (mode == ModeApply && !confirmed))
                return await Finish(host, item, mode, ApplyResults.NotReady, input.Request, null, l.Text("Apply.NotReady"), null);
            if (mode == ModeApply && item.Application is { Result: ApplyResults.Activated } && prep.NewSource is not null
                && item.Application.History.LastOrDefault()?.Mode == ModeApply)
                return new ApplyItemReport(item.N, item.ObjectName, ApplyResults.Activated, l.Text("Apply.AlreadyApplied"));
        }
        else if (item.Application?.BackupSource is null && prep is null)
            return await Finish(host, item, mode, ApplyResults.NotReady, input.Request, null, l.Text("Apply.NoBackup"), null);
        try
        {
            return IsDdl(item, prep)
                ? await DdicItemAsync(input, host, item, mode)
                : await AbapItemAsync(input, host, item, mode);
        }
        catch (InvalidOperationException ex)
        {
            return await Finish(host, item, mode, ApplyResults.GenericError, input.Request, null, ex.Message, null);
        }
    }

    private static bool IsDdl(RemediationItem item, ItemPreparation? prep) =>
        prep?.SourceUri?.StartsWith("ddic://", StringComparison.Ordinal) == true
        || (prep is null && item.ObjectType.ToUpperInvariant() is "STRU" or "TABL" && item.Application?.BackupSource is not null);

    private SourceCache? CacheFor(ApplyInput input, string? uri) =>
        uri is null ? null : input.Caches.FirstOrDefault(c => string.Equals(c.AdtUri, uri, StringComparison.OrdinalIgnoreCase));

    private static DateOnly DateOf(ItemProposal p, DateTimeOffset fallback) => DateOnly.FromDateTime((p.ProposedAt ?? fallback).LocalDateTime);

    // ---------------------------------------------------------------- ABAP

    private async Task<ApplyItemReport> AbapItemAsync(ApplyInput input, EngineHost host, RemediationItem item, string mode)
    {
        if (mode == ModeRollback)
            return await RollbackAbapAsync(input, host, item);
        var prep = item.Preparation!;
        var proposal = item.Proposal!;
        var date = DateOf(proposal, clock.UtcNow);
        var mainName = item.ObjectName.ToUpperInvariant();

        // objetos relacionados primeiro (interface antes da classe)
        var related = (prep.Related ?? []).OrderBy(r => r.ObjectType == "INTF" ? 0 : 1).ToList();
        List<string> notes = [];
        List<RelatedBackup> relatedBackups = [];
        foreach (var r in related)
        {
            var rc = CacheFor(input, r.SourceUri);
            var blocks = proposal.Blocks.Where(b => b.RelatedObject == r.ObjectName.ToUpperInvariant()).ToList();
            if (rc is null)
                return await Finish(host, item, mode, ApplyResults.GenericError, input.Request, null, l.Text("Apply.RelatedNoCache", r.ObjectName), null);
            var res = await ProcessSourceAsync(input, host, item, mode, rc, blocks, r.ObjectName, r.ObjectType, date, null, mode == ModeApply, null);
            notes.Add($"{r.ObjectName}: {res.Result}");
            if (mode == ModeApply && res.Backup is not null)
                relatedBackups.Add(new RelatedBackup(r.ObjectName, r.ObjectType, r.SourceUri, res.Backup));
            if (res.Stop || res.Result != (mode == ModeApply ? ApplyResults.Activated : ApplyResults.Simulated))
                return await Finish(host, item, mode, res.Stop ? "ERRO_BLOQUEIO" : res.Result, input.Request, null, $"{r.ObjectName}: {res.Detail}", res.Messages, res.Stop ? res.Detail : null,
                    relatedBackups: relatedBackups);
        }

        var cache = CacheFor(input, prep.SourceUri);
        if (cache is null)
            return await Finish(host, item, mode, ApplyResults.GenericError, input.Request, null, l.Text("Apply.NoCache", prep.Source ?? string.Empty), null);
        var mainBlocks = proposal.Blocks.Where(b => b.RelatedObject is null || b.RelatedObject == mainName).ToList();
        var main = await ProcessSourceAsync(input, host, item, mode, cache, mainBlocks, item.ObjectName, item.ObjectType, date, item, mode == ModeApply, prep);
        var detail = string.Join("; ", notes.Append(main.Detail ?? string.Empty).Where(s => s.Length > 0));
        return await Finish(host, item, mode, main.Stop ? "ERRO_BLOQUEIO" : main.Result, input.Request, main.Backup, detail, main.Messages, main.Stop ? main.Detail : null,
            relatedBackups: relatedBackups);
    }

    private sealed record SourceStep(string Result, string? Detail, string? Backup, IReadOnlyList<string>? Messages, bool Stop);

    /// <summary>Lê a versão ativa, confere com o fonte lido, faz o backup, monta o fonte novo e (se <paramref name="write"/>) grava e ativa.</summary>
    private async Task<SourceStep> ProcessSourceAsync(
        ApplyInput input, EngineHost host, RemediationItem item, string mode, SourceCache cache, IReadOnlyList<ProposalBlock> blocks, string objectName,
        string objectType, DateOnly date, RemediationItem? main, bool write, ItemPreparation? mainPrep)
    {
        var active = await reader.GetSourceAsync(host.Connection, cache.AdtUri, true, host.Ct);
        if (active.IsFailure)
            return new SourceStep(ApplyResults.GenericError, l.Text("Apply.ReadActiveFailed", active.Error.Code), null, null, false);
        if (!SourceNormalizer.SameSource(active.Value, cache.Source))
            return new SourceStep(ApplyResults.SourceChanged, l.Text("Apply.SourceChanged"), null, null, false);

        List<PatchSpec> specs = [];
        foreach (var b in blocks)
        {
            var (spec, code, arg) = ProposalBlocks.ToSpec(b, input.AuthorTag, date, false);
            if (spec is null)
                return new SourceStep(ApplyResults.PreparationError, l.Text("Apply.PreparationError", code ?? string.Empty, arg ?? string.Empty), null, null, false);
            specs.Add(spec);
        }
        var patch = SourcePatcher.Patch(cache.Source, specs, cache.IncludeName);
        if (!patch.IsOk)
        {
            // trecho já aplicado (itens do mesmo bloco): o bloco marcado já está no fonte
            if (specs.All(s => ContainsLines(cache.Source, s.Block)))
                return new SourceStep(write ? ApplyResults.Activated : ApplyResults.NothingToDo, l.Text("Apply.BlockAlreadyPresent"), active.Value, null, false);
            return new SourceStep(ApplyResults.PreparationError, l.Text("Apply.PreparationError", patch.ErrorCode ?? string.Empty, string.Join(",", patch.Occurrences)), null, null, false);
        }
        var newSource = patch.NewSource!;
        if (main is not null && mainPrep is not null && newSource != mainPrep.NewSource)
        {
            var updated = mainPrep with { NewSource = newSource, Diff = patch.Diff, Ranges = patch.Ranges, PreparedAt = clock.UtcNow };
            await host.MutateAsync(() => main.SetPreparation(updated));
        }
        if (!write)
            return new SourceStep(ApplyResults.Simulated, null, active.Value, null, false);

        var objectUri = SapUris.ObjectUri(cache.AdtUri);
        var request = new SourceWriteRequest(objectUri, SapUris.ObjectName(objectUri), cache.AdtUri, newSource, input.Request!,
            SapUris.ActivationContext(objectUri, objectType, objectName));
        var outcome = await writer.WriteSourceAsync(host.Connection, request, host.Ct);
        if (outcome.IsFailure)
            return new SourceStep(ApplyResults.GenericError, outcome.Error.Code, active.Value, null, outcome.Error.Type == ErrorType.Forbidden);
        var o = outcome.Value;
        if (o.IsActivated)
            await host.MutateAsync(() => cache.Refresh(newSource, clock.UtcNow));
        return new SourceStep(o.Result, o.Error ?? o.Observation, active.Value, o.Messages, false);
    }

    private static bool ContainsLines(string source, IReadOnlyList<string> block)
    {
        var lines = PyText.Normalize(PyText.SplitLines(source));
        var needle = PyText.Normalize(block);
        for (var i = 0; i + needle.Count <= lines.Count; i++)
            if (lines.Skip(i).Take(needle.Count).SequenceEqual(needle))
                return true;
        return false;
    }

    /// <summary>
    /// Rollback ABAP: o objeto principal volta ao backup e, depois dele, cada objeto relacionado ao seu próprio backup (<c>aplicar_adt.ps1 -Rollback -Objeto</c>
    /// do relacionado), na ordem inversa da aplicação (a interface foi aplicada primeiro, volta por último). Resultado = o do principal, ou o primeiro
    /// que não reverteu.
    /// </summary>
    private async Task<ApplyItemReport> RollbackAbapAsync(ApplyInput input, EngineHost host, RemediationItem item)
    {
        var app = item.Application;
        var relatedBackups = app?.RelatedBackups ?? [];
        if (app?.BackupSource is null && relatedBackups.Count == 0)
            return await Finish(host, item, ModeRollback, ApplyResults.NotReady, input.Request, null, l.Text("Apply.NoBackup"), null);
        var request = input.Request ?? app?.Request ?? string.Empty;
        string result = ApplyResults.RollbackActivated;
        List<string> notes = [];
        List<string>? messages = null;
        string? decision = null;

        async Task<ApplyItemReport?> Revert(SourceCache? cache, string backup, string name, string type, bool isMain)
        {
            if (cache is null)
                return await Finish(host, item, ModeRollback, ApplyResults.NotReady, input.Request, null, l.Text(isMain ? "Apply.NoBackup" : "Apply.RelatedNoCache", name), null);
            var objectUri = SapUris.ObjectUri(cache.AdtUri);
            var write = new SourceWriteRequest(objectUri, SapUris.ObjectName(objectUri), cache.AdtUri, backup, request, SapUris.ActivationContext(objectUri, type, name), Rollback: true);
            var outcome = await writer.WriteSourceAsync(host.Connection, write, host.Ct);
            if (outcome.IsFailure)
                return await Finish(host, item, ModeRollback, "ERRO_BLOQUEIO", input.Request, null, outcome.Error.Code, null, outcome.Error.Code);
            var o = outcome.Value;
            if (o.IsActivated)
                await host.MutateAsync(() => cache.Refresh(backup, clock.UtcNow));
            notes.Add(isMain ? (o.Error ?? o.Observation ?? o.Result) : $"{name}: {o.Result}");
            if (o.Messages is { Count: > 0 })
                (messages ??= []).AddRange(o.Messages);
            if (isMain)
                result = o.Result;
            else if (result == ApplyResults.RollbackActivated && o.Result != ApplyResults.RollbackActivated)
                result = o.Result;
            if (o.Result == ApplyResults.RollbackActivated)
                decision = ApplyResults.DecisionRollback;
            return null;
        }

        if (app?.BackupSource is { } main)
            if (await Revert(CacheFor(input, item.Preparation?.SourceUri), main, item.ObjectName, item.ObjectType, true) is { } stop)
                return stop;
        foreach (var r in relatedBackups.AsEnumerable().Reverse())
            if (await Revert(CacheFor(input, r.SourceUri), r.BackupSource, r.ObjectName, r.ObjectType, false) is { } stop)
                return stop;
        return await Finish(host, item, ModeRollback, result, input.Request, null, string.Join("; ", notes.Where(n => !string.IsNullOrEmpty(n))), messages, decision: decision);
    }

    // ---------------------------------------------------------------- DDIC (estrutura/tabela)

    private async Task<ApplyItemReport> DdicItemAsync(ApplyInput input, EngineHost host, RemediationItem item, string mode)
    {
        var kind = item.ObjectType.Equals("TABL", StringComparison.OrdinalIgnoreCase) ? DdicKind.Table : DdicKind.Structure;
        var state = await ddicWriter.ReadObjectAsync(host.Connection, kind, item.ObjectName, host.Ct);
        if (state.IsFailure)
            return await Finish(host, item, mode, ApplyResults.GenericError, input.Request, null, l.Text("Apply.ReadActiveFailed", state.Error.Code), null);
        string text;
        string create = "alterar";
        if (mode == ModeRollback)
        {
            var last = item.Application?.History.LastOrDefault(h => h.Mode == ModeApply);
            if (last?.DdicAction == ApplicationAttempt.DdicCreate)
                return await Finish(host, item, mode, ApplyResults.RollbackUnavailable, input.Request, null, l.Text("Apply.RollbackUnavailable"), null);
            if (item.Application?.BackupSource is null)
                return await Finish(host, item, mode, ApplyResults.NotReady, input.Request, null, l.Text("Apply.NoBackup"), null);
            text = item.Application.BackupSource;
        }
        else
        {
            var proposal = item.Proposal!;
            var date = DateOf(proposal, clock.UtcNow);
            if (state.Value.Exists)
            {
                List<PatchSpec> specs = [];
                foreach (var b in proposal.Blocks)
                {
                    var (spec, _, _) = ProposalBlocks.ToSpec(b, input.AuthorTag, date, true);
                    if (spec is not null)
                        specs.Add(spec);
                }
                var patch = SourcePatcher.Patch(state.Value.Source!, specs, item.ObjectName.ToUpperInvariant());
                if (!patch.IsOk)
                    return await Finish(host, item, mode, ApplyResults.SourceChanged, input.Request, null, l.Text("Apply.SourceChanged"), null);
                text = patch.NewSource!;
                if (SourceNormalizer.SameSource(state.Value.Source!, text))
                    return await Finish(host, item, mode, ApplyResults.NothingToDo, input.Request, null, null, null, ddicAction: ApplicationAttempt.DdicAlter);
                if (kind == DdicKind.Table && !input.AllowTableStructure)
                {
                    var c0 = string.Join(',', DdicApplyRules.Fields(state.Value.Source!));
                    var c1 = string.Join(',', DdicApplyRules.Fields(text));
                    if (c0 != c1)
                        return await Finish(host, item, mode, ApplyResults.TableStructureAborted, input.Request, null, l.Text("Apply.TableStructure", c0, c1), null);
                }
                await host.MutateAsync(() => item.SetPreparation(item.Preparation! with { NewSource = text, Diff = patch.Diff, Ranges = patch.Ranges, PreparedAt = clock.UtcNow }));
            }
            else
            {
                create = "criar";
                text = item.Preparation!.NewSource!;
                if (!input.DdicCreate.TryGetValue(item.N, out var creation))
                    return await Finish(host, item, mode, ApplyResults.ValidationError, input.Request, null, l.Text("Apply.DdicCreateData"), null);
                var v = await ddicWriter.ValidateNameAsync(host.Connection, kind, item.ObjectName, creation.Package, creation.Description, host.Ct);
                if (v.IsFailure)
                    return await Finish(host, item, mode, "ERRO_BLOQUEIO", input.Request, null, v.Error.Code, null, v.Error.Code);
                if (!v.Value.Ok)
                    return await Finish(host, item, mode, ApplyResults.ValidationError, input.Request, null, $"{v.Value.Severity} {v.Value.Text}".Trim(), null);
            }
            if (mode == ModeSimulation)
                return await Finish(host, item, mode, ApplyResults.Simulated, input.Request, state.Value.Source, null, null, ddicAction: create);
        }
        var creating = create == "criar";
        input.DdicCreate.TryGetValue(item.N, out var cr);
        var req = new DdicWriteRequest(kind, item.ObjectName, text, input.Request ?? string.Empty, creating, cr?.Package, cr?.Description, input.Responsible, mode == ModeRollback);
        var outcome = await ddicWriter.WriteAsync(host.Connection, req, host.Ct);
        if (outcome.IsFailure)
            return await Finish(host, item, mode, "ERRO_BLOQUEIO", input.Request, null, outcome.Error.Code, null, outcome.Error.Code);
        var o = outcome.Value;
        return await Finish(host, item, mode, o.Result, input.Request, mode == ModeRollback ? null : state.Value.Source, o.Error,
            o.Messages, ddicAction: create, decision: o.Result == DdicWriteOutcome.RollbackActivated ? ApplyResults.DecisionRollback : null);
    }

    // ---------------------------------------------------------------- registro (cmd_aplicacao)

    private async Task<ApplyItemReport> Finish(
        EngineHost host, RemediationItem item, string mode, string result, string? request, string? backup, string? detail, IReadOnlyList<string>? messages,
        string? stopCode = null, string? decision = null, IReadOnlyList<RelatedBackup>? relatedBackups = null, string? ddicAction = null)
    {
        var at = clock.UtcNow;
        var previous = item.Application;
        var shown = stopCode is null ? result : "ERRO";
        var attempt = new ApplicationAttempt(at, mode, shown, request, detail, ddicAction);
        // backups de relacionados: os novos substituem os do mesmo objeto; os anteriores (rollback futuro) permanecem
        var related = relatedBackups is { Count: > 0 }
            ? [.. (previous?.RelatedBackups ?? []).Where(p => !relatedBackups.Any(r => r.ObjectName == p.ObjectName)), .. relatedBackups]
            : previous?.RelatedBackups;
        var app = new ItemApplication(shown, request, backup ?? previous?.BackupSource, decision ?? previous?.ErrorDecision, messages ?? previous?.Messages,
            [.. previous?.History ?? [], attempt], related);
        await host.MutateAsync(() =>
        {
            item.SetApplication(app);
            if (mode != ModeSimulation && item.CorrectionSheetMd is { } sheet)
                item.SetCorrectionSheet(sheet.TrimEnd() + "\n\n" + $"{CorrectionSheet.ApplicationPrefix} {shown} · request {request} · {at:s}"
                                        + (decision is null ? string.Empty : $" · decisão: {decision}") + "\n");
        });
        if (stopCode is not null)
            await host.LogAsync(EventLevel.Error, stopCode, null, $"item {item.N}");
        return new ApplyItemReport(item.N, item.ObjectName, stopCode is null ? result : "ERRO_BLOQUEIO", stopCode ?? detail);
    }
}
