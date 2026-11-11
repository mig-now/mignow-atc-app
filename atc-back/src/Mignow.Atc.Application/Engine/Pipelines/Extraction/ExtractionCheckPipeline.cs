using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Engine.Steps.Extraction;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Engine.Pipelines.Extraction;

/// <summary>Verifica a última execução concluída do ATC (série ZMIG_/Z_MIG_) sem extrair: um passo, resultado no run.</summary>
public sealed class ExtractionCheckPipeline : IRunPipeline
{
    public RunKind Kind => RunKind.ExtractionCheck;

    public IReadOnlyList<Type> StepTypes { get; } = [typeof(FindLatestAtcStep)];
}
