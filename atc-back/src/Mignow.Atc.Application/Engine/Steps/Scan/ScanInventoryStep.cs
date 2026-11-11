using System.Text.Json.Nodes;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Modules.Scan;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Ports.Sap.Scan;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Domain.Scan;

namespace Mignow.Atc.Application.Engine.Steps.Scan;

/// <summary>
/// Inventário do escopo Z/Y (TADIR, somente leitura) e divisão em lotes por peso. Refazer o inventário descarta lotes,
/// ocorrências e resumo da varredura (os lotes mudam). Idempotente.
/// </summary>
public sealed class ScanInventoryStep(IMignowScanner scanner) : IRunStep
{
    public string Code => ScanStepCodes.Inventory;

    public async Task<StepOutcome> ExecuteAsync(RunContext ctx)
    {
        await ctx.ReportProgressAsync(0, 1);
        var scan = await ScanRunSupport.LoadScanAsync(ctx);
        if (scan is null)
            return StepOutcome.Stop("Scan.NotFound");
        var includeGenerated = ctx.Parameters<ScanInventoryParameters>().IncludeGenerated ?? ScanRunSupport.IncludeGenerated(scan.ScopeCountsJson);
        var types = ScanInventoryBuilder.DefaultTypes;
        var rows = await scanner.InventoryAsync(ctx.SapConnection, types, ctx.Ct);
        if (rows.IsFailure)
            return StepOutcome.Stop(rows.Error.Code, Json.Args(("message", rows.Error.Message)));
        var inv = ScanInventoryBuilder.Build(rows.Value, includeGenerated, ctx.Settings.MignowWeightPerBatch, types, ScanCodeGenerator.DefaultClass);
        var counts = new JsonObject
        {
            ["sid"] = ctx.SapSystem.Sid,
            ["mandante"] = ctx.SapSystem.Client,
            ["gerado_em"] = ctx.Clock.UtcNow.ToLocalTime().ToString("yyyy-MM-dd'T'HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
            ["tipos"] = new JsonArray([.. types.Select(t => (JsonNode?)JsonValue.Create(t))]),
            ["tadir_lidos"] = inv.TadirRead,
            ["escopo"] = inv.ScopeCount,
            ["por_tipo"] = new JsonObject(inv.ByType.Select(kv => KeyValuePair.Create(kv.Key, (JsonNode?)JsonValue.Create(kv.Value)))),
            ["excluidos"] = new JsonObject { ["tmp"] = inv.ExcludedTmp, ["marcados_exclusao"] = inv.ExcludedDeleted, ["gerados"] = inv.ExcludedGenerated },
            ["incluir_gerados"] = inv.IncludeGenerated,
            ["peso_por_lote"] = inv.WeightPerBatch,
            ["lotes"] = inv.Batches.Count,
        };
        var scanId = scan.Id;
        await ctx.WithDbAsync(async () =>
        {
            foreach (var old in await ctx.Store.ListAsync(ctx.Store.Query<MignowOccurrence>().Where(o => o.ScanId == scanId), CancellationToken.None))
                ctx.Store.Remove(old);
            scan.SetBatches(inv.Batches);
            scan.SetScope(counts.ToJsonString());
            scan.SetSummary(null);
        });
        await ctx.SaveAsync();
        await ctx.SetResultAsync(new { scope = inv.ScopeCount, byType = inv.ByType, excluded = new { tmp = inv.ExcludedTmp, deleted = inv.ExcludedDeleted, generated = inv.ExcludedGenerated }, lots = inv.Batches.Count, weightPerBatch = inv.WeightPerBatch });
        await ctx.ReportProgressAsync(1, 1);
        await ctx.LogAsync(EventLevel.Info, "Scan.InventoryDone", Json.Args(("objects", inv.ScopeCount), ("lots", inv.Batches.Count)));
        return StepOutcome.Done;
    }
}
