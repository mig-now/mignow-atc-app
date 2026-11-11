using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Modules.Extraction;
using Mignow.Atc.Application.Ports.Sap.Extraction;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Engine.Steps.Extraction;

/// <summary>
/// Passo comum de <c>ExtractionCheck</c> e <c>Extraction</c> (<c>extrair.ps1 -SoVerificar</c>): acha a última execução concluída,
/// avisa das mais novas em andamento e confere se já foi extraída. Guarda o resultado no run (o próximo passo lê dele).
/// Idempotente: só lê SAP e banco.
/// </summary>
public sealed class FindLatestAtcStep(IAtcExtractionReader reader, IStringLocalizer<Errors> l) : IRunStep
{
    public string Code => ExtractionStepCodes.FindLatest;

    public async Task<StepOutcome> ExecuteAsync(RunContext ctx)
    {
        await ctx.ReportProgressAsync(0, 1);
        var selection = await AtcLatestFinder.SelectAsync(reader, l, ctx.SapConnection, ctx.Ct);
        if (selection.IsFailure)
        {
            await ctx.SetResultAsync(new { ok = false, code = selection.Error.Code, message = selection.Error.Message });
            return StepOutcome.Stop(selection.Error.Code, Json.Args(("message", selection.Error.Message)));
        }
        var latest = selection.Value.Latest!;
        var existing = await ctx.WithDbAsync(() => ExtractionQueries.FindByDisplayIdAsync(ctx.Store, ctx.SapSystem.Id, latest.DisplayId, false, ctx.Ct));
        var info = AtcLatestFinder.Describe(selection.Value, ctx.SapSystem, existing);
        var checkOnly = ctx.Run.Kind == RunKind.ExtractionCheck;
        var force = !checkOnly && ctx.Parameters<ExtractionRunParameters>().Force;
        var situation = checkOnly ? ExtractionRunResult.Checked : existing is not null && !force ? ExtractionRunResult.AlreadyExtracted : ExtractionRunResult.Found;
        await ctx.SetResultAsync(new ExtractionRunResult(situation, info, existing?.Guid));

        var args = Json.Args(("displayId", info.DisplayId), ("series", info.Series));
        await ctx.LogAsync(EventLevel.Info, "Extraction.LatestFound", args);
        foreach (var p in info.InProgress)
            await ctx.LogAsync(EventLevel.Warning, "Extraction.NewerInProgress", Json.Args(("displayId", p.DisplayId), ("series", p.Series)));
        await ctx.ReportProgressAsync(1, 1);
        return situation == ExtractionRunResult.AlreadyExtracted ? StepOutcome.Info("Extraction.AlreadyExtracted", args) : StepOutcome.Info("Extraction.LatestFound", args);
    }
}
