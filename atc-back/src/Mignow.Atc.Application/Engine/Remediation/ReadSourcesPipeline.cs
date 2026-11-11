using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Engine.Remediation;

/// <summary>Leitura de fontes: escolhe os objetos do lote e lê (paralelo, com cache, standard no fim da fila e só com confirmação).</summary>
public sealed class ReadSourcesPipeline : IRunPipeline
{
    public RunKind Kind => RunKind.ReadSources;

    public IReadOnlyList<Type> StepTypes { get; } = [typeof(ReadSourcesPlanStep), typeof(ReadSourcesFetchStep)];
}
