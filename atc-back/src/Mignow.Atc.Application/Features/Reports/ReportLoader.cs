using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Features.Settings;
using Mignow.Atc.Application.Modules.Report;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Ports.Time;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Domain.Exemption;
using Mignow.Atc.Domain.Extraction;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Domain.Remediation;
using Mignow.Atc.Domain.Report;

namespace Mignow.Atc.Application.Features.Reports;

/// <summary>Contexto lido do banco para o relatório de um cliente: o ambiente escolhido e o <see cref="ReportInput"/>.</summary>
internal sealed record LoadedReport(ReportInput Input, IReadOnlyList<SessionData> Sessions);

/// <summary>Lê do banco o que o <c>coletar.py</c> lia da pasta do cliente (somente leitura).</summary>
internal static class ReportLoader
{
    public static async Task<Customer?> FindCustomerAsync(IDataStore store, Guid id, CancellationToken ct) =>
        await store.FirstOrDefaultAsync(store.Read<Customer>().Where(c => c.Guid == id), ct);

    /// <summary>Ambiente do relatório: o pedido, ou o do cliente com a sessão mais recente.</summary>
    public static async Task<Result<SapSystem>> FindSystemAsync(IDataStore store, IStringLocalizer<Errors> l, Customer customer, Guid? sapSystemId, CancellationToken ct)
    {
        if (sapSystemId is not null)
        {
            var s = await store.FirstOrDefaultAsync(store.Read<SapSystem>().Where(x => x.Guid == sapSystemId && x.CustomerId == customer.Id), ct);
            return s is null ? l.NotFound("Report.SystemNotFound") : s;
        }
        var systemId = await store.FirstOrDefaultAsync(store.Read<RemediationSession>().Where(x => x.CustomerId == customer.Id)
            .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id).Select(x => (int?)x.SapSystemId), ct);
        if (systemId is null)
            return l.NotFound("Report.NoSession");
        var system = await store.FirstOrDefaultAsync(store.Read<SapSystem>().Where(x => x.Id == systemId), ct);
        return system is null ? l.NotFound("Report.SystemNotFound") : system;
    }

    public static async Task<IReadOnlyList<SessionData>> LoadSessionsAsync(IDataStore store, int systemId, CancellationToken ct)
    {
        var sessions = await store.ListAsync(store.Read<RemediationSession>().Where(s => s.SapSystemId == systemId).OrderBy(s => s.Cycle).ThenBy(s => s.Id), ct);
        var ids = sessions.Select(s => s.Id).ToList();
        var items = await store.ListAsync(store.Read<RemediationItem>().Where(i => ids.Contains(i.SessionId)).OrderBy(i => i.SessionId).ThenBy(i => i.N), ct);
        return [.. sessions.Select(s => new SessionData(s, items.Where(i => i.SessionId == s.Id).ToList(), s.CreatedAt))];
    }

    public static async Task<Result<LoadedReport>> LoadAsync(IDataStore store, IStringLocalizer<Errors> l, IDateTimeProvider clock, Guid customerId, Guid? sapSystemId,
        CancellationToken ct)
    {
        var customer = await FindCustomerAsync(store, customerId, ct);
        if (customer is null)
            return l.NotFound("Customer.NotFound");
        var system = await FindSystemAsync(store, l, customer, sapSystemId, ct);
        if (system.IsFailure)
            return system.Error;
        var sys = system.Value;
        var sessions = await LoadSessionsAsync(store, sys.Id, ct);
        if (sessions.Count == 0)
            return l.NotFound("Report.NoSession");
        var settings = await SettingsStore.LoadAsync(store, false, ct);
        var extractions = await store.ListAsync(store.Read<AtcExtraction>().Where(e => e.SapSystemId == sys.Id).OrderBy(e => e.ExtractedAt), ct);

        var itemN = sessions.SelectMany(s => s.Items).ToDictionary(i => i.Id, i => i.N);
        var batches = await store.ListAsync(store.Read<ExemptionBatch>().Where(b => b.SapSystemId == sys.Id).OrderBy(b => b.Id), ct);
        var batchIds = batches.Select(b => b.Id).ToList();
        var requests = await store.ListAsync(store.Read<ExemptionRequest>().Where(r => batchIds.Contains(r.BatchId)).OrderBy(r => r.Seq), ct);
        var batchData = batches.Select(b => new ExemptionBatchData(b, b.CreatedAt,
            [.. requests.Where(r => r.BatchId == b.Id && itemN.ContainsKey(r.ItemId)).Select(r => new ExemptionRequestData(r, itemN[r.ItemId]))])).ToList();

        var runs = await store.ListAsync(store.Read<Run>().Where(r => r.SapSystemId == sys.Id).OrderBy(r => r.QueuedAt), ct);
        var runIds = runs.Select(r => r.Id).ToList();
        var calls = await store.ListAsync(store.Read<RunAiCall>().Where(c => runIds.Contains(c.RunId)), ct);
        var runData = runs.Select(r => new RunWithCalls(r, calls.Where(c => c.RunId == r.Id).ToList())).ToList();

        var highlights = await store.FirstOrDefaultAsync(store.Read<ReportHighlights>().Where(h => h.CustomerId == customer.Id), ct);
        var input = new ReportInput(customer, sys, settings, sessions, batchData, extractions, runData, highlights, clock.UtcNow, TimeZoneInfo.Local);
        return new LoadedReport(input, sessions);
    }
}
