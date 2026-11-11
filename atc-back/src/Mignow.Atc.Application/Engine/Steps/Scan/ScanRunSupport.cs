using System.Text.Json.Nodes;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Services;
using Mignow.Atc.Application.Modules.Scan;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Ports.Sap.Scan;
using Mignow.Atc.Domain.Scan;

namespace Mignow.Atc.Application.Engine.Steps.Scan;

/// <summary>Auxiliares comuns dos passos da varredura (carga da varredura, contagens do escopo, verificações antes de gravar).</summary>
public static class ScanRunSupport
{
    public static Task<MignowScan?> LoadScanAsync(RunContext ctx) =>
        ctx.WithDbAsync(() =>
        {
            var id = ctx.Run.SubjectId;
            return ctx.Store.FirstOrDefaultAsync(ctx.Store.Query<MignowScan>().Where(s => s.Guid == id), ctx.Ct);
        });

    public static bool IncludeGenerated(string? scopeJson) =>
        scopeJson is not null && JsonNode.Parse(scopeJson) is JsonObject o && o["incluir_gerados"] is { } n && n.GetValue<bool>();

    public static ScanInventoryCounts Counts(string? scopeJson)
    {
        var o = scopeJson is null ? new JsonObject() : JsonNode.Parse(scopeJson)!.AsObject();
        var byType = new Dictionary<string, int>(StringComparer.Ordinal);
        if (o["por_tipo"] is JsonObject t)
            foreach (var (k, v) in t)
                byType[k] = v!.GetValue<int>();
        var ex = o["excluidos"] as JsonObject;
        return new ScanInventoryCounts(Int(o["escopo"]), byType, Int(ex?["tmp"]), Int(ex?["marcados_exclusao"]), Int(ex?["gerados"]));
    }

    private static int Int(JsonNode? n) => n is null ? 0 : n.GetValue<int>();

    /// <summary>Lotes pendentes (a executar) segundo a seleção e o "refazer".</summary>
    public static IReadOnlyList<int> PendingLots(MignowScan scan, ScanExecuteParameters p)
    {
        var all = scan.Batches.Select(b => b.Number).ToList();
        var selected = ScanProcessPlanner.ParseLots(p.Lots, all);
        return [.. selected.Where(n => p.Redo || !scan.Batches.First(b => b.Number == n).IsOk)];
    }

    /// <summary>Processos em paralelo: o pedido (ou o padrão das configurações), no máximo 9 e dentro do limite de chamadas do ADT (cada processo segura um espaço).</summary>
    public static int Parallel(RunContext ctx, ScanExecuteParameters p, SapConcurrencyOptions limits) =>
        Math.Clamp(p.Parallel ?? ctx.Settings.MignowParallel, 1, Math.Min(ScanProcessPlanner.MaxParallel, limits.MaxMignowParallel));

    /// <summary>Simulação: T000 do mandante e, por classe, nome livre e validação no ADT. Devolve o objeto do resultado e se está ok.</summary>
    public static async Task<(Result<ScanSimulation> Simulation, bool Ok, object Result)> PreflightAsync(
        IMignowScanner scanner, RunContext ctx, MignowScan scan, IReadOnlyList<int> pending, int parallel)
    {
        var groups = ScanProcessPlanner.RoundRobin(pending, parallel);
        var classes = Enumerable.Range(1, Math.Max(1, groups.Count)).Select(w => ScanProcessPlanner.ProcessClass(ScanCodeGenerator.DefaultClass, w)).ToList();
        var sim = await scanner.SimulateAsync(ctx.SapConnection, classes, ctx.Ct);
        if (sim.IsFailure)
            return (sim, false, new { ok = false, code = sim.Error.Code, message = sim.Error.Message });
        var s = sim.Value;
        var classesOk = s.Classes.All(c => !c.Exists && c.ValidationOk);
        var ok = s.ClientOk && classesOk && scan.Batches.Count > 0;
        var result = new
        {
            ok,
            mode = "simulation",
            clientCategory = s.ClientCategory,
            crossClientChange = s.NoChangeIndicator,
            clientOk = s.ClientOk,
            parallel = groups.Count,
            lotsToRun = pending,
            lotsAlreadyOk = scan.Batches.Where(b => b.IsOk).Select(b => b.Number).ToList(),
            processes = groups.Select((g, i) => new { className = classes[i], lots = g }).ToList(),
            classes = s.Classes,
        };
        return (sim, ok, result);
    }
}
