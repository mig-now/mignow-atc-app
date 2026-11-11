using Microsoft.Extensions.Options;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Ports.Sap.Scan;
using Mignow.Atc.Application.Services;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Engine.Steps.Scan;

/// <summary>Simulação (somente leitura): papel do mandante (T000), nomes das classes livres e validação no ADT. Nada é gravado.</summary>
public sealed class ScanSimulateStep(IMignowScanner scanner, IOptions<SapConcurrencyOptions> limits) : IRunStep
{
    public string Code => ScanStepCodes.Simulate;

    public async Task<StepOutcome> ExecuteAsync(RunContext ctx)
    {
        await ctx.ReportProgressAsync(0, 1);
        var scan = await ScanRunSupport.LoadScanAsync(ctx);
        if (scan is null)
            return StepOutcome.Stop("Scan.NotFound");
        if (scan.Batches.Count == 0)
            return StepOutcome.Stop("Scan.NoInventory");
        var p = ctx.Parameters<ScanExecuteParameters>();
        var pending = ScanRunSupport.PendingLots(scan, p);
        var (sim, ok, result) = await ScanRunSupport.PreflightAsync(scanner, ctx, scan, pending, ScanRunSupport.Parallel(ctx, p, limits.Value));
        await ctx.SetResultAsync(result);
        if (sim.IsFailure)
            return StepOutcome.Stop(sim.Error.Code, Json.Args(("message", sim.Error.Message)));
        await ctx.ReportProgressAsync(1, 1);
        if (!ok)
            return StepOutcome.Stop("Scan.SimulationFailed");
        await ctx.LogAsync(EventLevel.Info, "Scan.SimulationOk", Json.Args(("category", sim.Value.ClientCategory), ("lots", pending.Count)));
        return StepOutcome.Info("Scan.SimulationOk");
    }
}
