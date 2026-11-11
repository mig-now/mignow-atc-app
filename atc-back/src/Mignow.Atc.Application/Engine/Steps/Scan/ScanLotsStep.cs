using Microsoft.Extensions.Options;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Modules.Scan;
using Mignow.Atc.Application.Services;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Engine.Steps.Scan;

/// <summary>Demais lotes pendentes em paralelo (rodízio entre N processos, cada um com a sua classe <c>ZCL_MIGNOW_ATC_SCAN_n</c>, apagada no final). Retomável: lotes já OK são pulados.</summary>
public sealed class ScanLotsStep(ScanLotRunner runner, IOptions<SapConcurrencyOptions> limits) : IRunStep
{
    public string Code => ScanStepCodes.Lots;

    public async Task<StepOutcome> ExecuteAsync(RunContext ctx)
    {
        var scan = await ScanRunSupport.LoadScanAsync(ctx);
        if (scan is null)
            return StepOutcome.Stop("Scan.NotFound");
        var p = ctx.Parameters<ScanExecuteParameters>();
        // a calibração acabou de rodar o lote 1 (inclusive no "refazer")
        var pending = ScanRunSupport.PendingLots(scan, p).Where(n => n != 1 || !p.Redo && !scan.Batches.First(b => b.Number == 1).IsOk).ToList();
        if (pending.Count == 0)
        {
            await ctx.ReportProgressAsync(1, 1);
            return StepOutcome.Info("Scan.NoPendingLots");
        }
        var groups = ScanProcessPlanner.RoundRobin(pending, ScanRunSupport.Parallel(ctx, p, limits.Value));
        var processes = groups.Select((g, i) => (ScanProcessPlanner.ProcessClass(ScanCodeGenerator.DefaultClass, i + 1), g)).ToList();
        await ctx.ReportProgressAsync(0, pending.Count);
        var outcome = await runner.RunAsync(ctx, scan, processes, p, scan.RulesSnapshotJson);
        await ctx.ReportProgressAsync(outcome.LotsDone.Count, pending.Count);
        await ctx.SetResultAsync(new
        {
            lots = pending,
            lotsOk = scan.Batches.Count(b => b.IsOk),
            lotsTotal = scan.Batches.Count,
            parallel = groups.Count,
            failed = outcome.FailedLots,
            processErrors = outcome.ProcessErrors,
            classesNotRemoved = outcome.ClassesNotRemoved,
        });
        if (outcome.ClassesNotRemoved.Count > 0)
            return StepOutcome.Stop("Scan.ClassNotRemoved", Json.Args(("classes", string.Join(", ", outcome.ClassesNotRemoved))));
        if (outcome.HasFailure)
            return StepOutcome.Stop("Scan.LotsFailed", Json.Args(("lots", string.Join(", ", outcome.FailedLots.Concat(outcome.ProcessErrors)))));
        return outcome.Cancelled ? StepOutcome.Cancel : StepOutcome.Done;
    }
}
