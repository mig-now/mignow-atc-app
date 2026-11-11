using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Features.Exemptions;
using Mignow.Atc.Domain.Exemption;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Engine.Steps.Exemption;

/// <summary>Carga do lote e histórico (porte do <c>resultado.py</c> da skill) comuns aos passos do módulo.</summary>
internal static class ExemptionStepSupport
{
    public static Task<ExemptionBatch> LoadBatchAsync(RunContext ctx) =>
        ctx.WithDbAsync(async () =>
        {
            var id = ctx.Parameters<ExemptionRunParameters>().BatchId;
            return await ctx.Store.FirstOrDefaultAsync(
                ctx.Store.Including(ctx.Store.Query<ExemptionBatch>(), nameof(ExemptionBatch.Requests)).Where(b => b.Guid == id), CancellationToken.None)
                ?? throw new InvalidOperationException("Lote de isenção não encontrado.");
        });

    /// <summary>Pedidos do lote em ordem de <c>seq</c>, restritos ao <c>seq</c> pedido no run.</summary>
    public static List<ExemptionRequest> Scope(RunContext ctx, ExemptionBatch batch)
    {
        var seq = ctx.Parameters<ExemptionRunParameters>().Seq;
        return [.. batch.Requests.Where(r => seq is not { Count: > 0 } || seq.Contains(r.Seq)).OrderBy(r => r.Seq)];
    }

    public static Task<Dictionary<int, RemediationItem>> ItemsAsync(RunContext ctx, IEnumerable<ExemptionRequest> requests)
    {
        var ids = requests.Select(r => r.ItemId).Distinct().ToArray();
        return ctx.WithDbAsync(async () =>
            (await ctx.Store.ListAsync(ctx.Store.Read<RemediationItem>().Where(i => ids.Contains(i.Id)), CancellationToken.None)).ToDictionary(i => i.Id));
    }

    /// <summary>Histórico do lote no <c>ResultJson</c> do run: quantidades, política e tempo (<c>historico.json</c> da skill).</summary>
    public static object History(RunContext ctx, ExemptionBatch batch, string? policy)
    {
        var rows = batch.Requests.Select(r => (r.Status, r.RequestXml != null)).ToList();
        var q = ExemptionStatusMath.Quantities(rows);
        var elapsed = ctx.Run.StartedAt is { } start ? (int)(ctx.Clock.UtcNow - start).TotalSeconds : 0;
        return new { quantities = q, totals = ExemptionStatusMath.Totals(rows), policy, elapsedSeconds = elapsed };
    }

    public static string? PolicyOf(ExemptionBatch batch)
    {
        var d = Mignow.Atc.Application.Dtos.Exemption.ExemptionViews.Element(batch.DiscoveryJson);
        return d is { } e && e.TryGetProperty("policy", out var p) && p.TryGetProperty("display", out var s) ? s.GetString() : null;
    }

    public static string Language(RunContext ctx) => ctx.SapConnection.Language;

    public static string Prefixed(string code, string detail) => Json.Args((code, detail));
}
