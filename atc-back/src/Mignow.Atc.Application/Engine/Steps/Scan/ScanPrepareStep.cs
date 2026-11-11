using Microsoft.Extensions.Options;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Ports.Sap.Scan;
using Mignow.Atc.Application.Services;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Engine.Steps.Scan;

/// <summary>Verificações antes de gravar (a simulação de novo, como a Etapa 5 da skill): mandante fora de produção e nomes livres. Falhou: nada foi gravado.</summary>
public sealed class ScanPrepareStep(IMignowScanner scanner, IOptions<SapConcurrencyOptions> limits) : IRunStep
{
    public string Code => ScanStepCodes.Prepare;

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
        if (pending.Count == 0)
        {
            await ctx.ReportProgressAsync(1, 1);
            return StepOutcome.Info("Scan.NoPendingLots");
        }
        var (sim, ok, result) = await ScanRunSupport.PreflightAsync(scanner, ctx, scan, pending, ScanRunSupport.Parallel(ctx, p, limits.Value));
        await ctx.SetResultAsync(result);
        if (sim.IsFailure)
            return StepOutcome.Stop(sim.Error.Code, Json.Args(("message", sim.Error.Message)));
        if (!ok)
        {
            var taken = sim.Value.Classes.FirstOrDefault(c => c.Exists);
            return taken is not null
                ? StepOutcome.Stop("Scan.ClassExists", Json.Args(("class", taken.ClassName), ("package", taken.Package)))
                : StepOutcome.Stop("Scan.SimulationFailed");
        }
        await ctx.LogAsync(EventLevel.Info, "Scan.ChecksOk", Json.Args(("category", sim.Value.ClientCategory), ("lots", pending.Count)));
        await ctx.ReportProgressAsync(1, 1);
        return StepOutcome.Done;
    }
}
