using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Modules.Scan;
using Mignow.Atc.Application.Ports.Sap.Scan;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Domain.Scan;

namespace Mignow.Atc.Application.Engine.Steps.Scan;

/// <summary>Remoção de resíduos (<c>-SoRemover</c>): apaga a classe base e <c>_1.._N</c> que tenham ficado no sistema, só se estiverem em <c>$TMP</c>.</summary>
public sealed class ScanCleanupStep(IMignowScanner scanner) : IRunStep
{
    public string Code => ScanStepCodes.Cleanup;

    public async Task<StepOutcome> ExecuteAsync(RunContext ctx)
    {
        await ctx.ReportProgressAsync(0, 1);
        var classes = ScanProcessPlanner.AllClasses(ScanCodeGenerator.DefaultClass, ScanProcessPlanner.MaxParallel);
        var removal = await scanner.RemoveResiduesAsync(ctx.SapConnection, classes, ctx.Ct);
        if (removal.IsFailure)
            return StepOutcome.Stop(removal.Error.Code, Json.Args(("message", removal.Error.Message)));
        var list = removal.Value;
        await ctx.SetResultAsync(new { ok = list.All(r => r.Removed), classes = list });
        var scan = await ScanRunSupport.LoadScanAsync(ctx);
        if (scan is not null)
        {
            await ctx.MutateAsync(() =>
            {
                var gone = list.Where(r => r.Removed).Select(r => r.ClassName).ToHashSet(StringComparer.OrdinalIgnoreCase);
                scan.SetBatches(scan.Batches.Select(b => gone.Contains(b.ClassName) ? b with { ClassRemoved = true } : b));
            });
            await ctx.SaveAsync();
        }
        await ctx.ReportProgressAsync(1, 1);
        var stuck = list.Where(r => !r.Removed).Select(r => r.ClassName).ToList();
        if (stuck.Count > 0)
            return StepOutcome.Stop("Scan.ClassNotRemoved", Json.Args(("classes", string.Join(", ", stuck))));
        await ctx.LogAsync(EventLevel.Info, "Scan.CleanupDone", Json.Args(("classes", list.Count(r => r.Situation == "removida"))));
        return StepOutcome.Done;
    }
}
