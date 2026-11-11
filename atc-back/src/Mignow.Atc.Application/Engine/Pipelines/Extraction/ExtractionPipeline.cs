using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Engine.Steps.Extraction;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Engine.Pipelines.Extraction;

/// <summary>Extração da última execução concluída do ATC: achar a execução → ler findings e complementos → gravar.</summary>
public sealed class ExtractionPipeline : IRunPipeline
{
    public RunKind Kind => RunKind.Extraction;

    public IReadOnlyList<Type> StepTypes { get; } = [typeof(FindLatestAtcStep), typeof(ExtractFindingsStep)];
}
