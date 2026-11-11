using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Modules.Scan;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Engine.Steps.Scan;

/// <summary>Calibração (Etapa 6 da skill): roda primeiro só o lote 1, sozinho, e registra o tempo (se estiver perto do limite de diálogo do SAP, refazer o inventário com peso menor).</summary>
public sealed class ScanCalibrateStep(ScanLotRunner runner) : IRunStep
{
    public string Code => ScanStepCodes.Calibrate;

    public async Task<StepOutcome> ExecuteAsync(RunContext ctx)
    {
        var scan = await ScanRunSupport.LoadScanAsync(ctx);
        if (scan is null)
            return StepOutcome.Stop("Scan.NotFound");
        var p = ctx.Parameters<ScanExecuteParameters>();
        var pending = ScanRunSupport.PendingLots(scan, p);
        if (!pending.Contains(1))
        {
            await ctx.ReportProgressAsync(1, 1);
            return StepOutcome.Info("Scan.CalibrationSkipped");
        }
        await ctx.ReportProgressAsync(0, 1);
        var outcome = await runner.RunAsync(ctx, scan, [(ScanProcessPlanner.ProcessClass(ScanCodeGenerator.DefaultClass, 1), [1])], p, scan.RulesSnapshotJson);
        await ctx.ReportProgressAsync(1, 1);
        if (outcome.Cancelled)
            return StepOutcome.Cancel;
        if (outcome.ClassesNotRemoved.Count > 0)
            return StepOutcome.Stop("Scan.ClassNotRemoved", Json.Args(("classes", string.Join(", ", outcome.ClassesNotRemoved))));
        if (outcome.HasFailure)
        {
            await ctx.SetResultAsync(new { calibration = outcome });
            return StepOutcome.Stop("Scan.LotsFailed", Json.Args(("lots", string.Join(", ", outcome.FailedLots.Concat(outcome.ProcessErrors)))));
        }
        var seconds = scan.Batches.First(b => b.Number == 1).Stats?.TimeSeconds;
        await ctx.LogAsync(EventLevel.Info, "Scan.CalibrationDone", Json.Args(("seconds", seconds)));
        return StepOutcome.Info("Scan.CalibrationDone", Json.Args(("seconds", seconds)));
    }
}
