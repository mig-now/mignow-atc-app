using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Engine.Steps.Scan;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Engine.Pipelines.Scan;

/// <summary>Verificações → lote 1 (calibração) → demais lotes em paralelo → cruzamento com o ATC.</summary>
public sealed class ScanExecutePipeline : IRunPipeline
{
    public RunKind Kind => RunKind.ScanExecute;

    public IReadOnlyList<Type> StepTypes { get; } = [typeof(ScanPrepareStep), typeof(ScanCalibrateStep), typeof(ScanLotsStep), typeof(ScanCrossStep)];
}
