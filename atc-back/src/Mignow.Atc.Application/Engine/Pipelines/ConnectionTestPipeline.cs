using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Engine.Steps;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Engine.Pipelines;

/// <summary>Exemplo funcional do motor: teste de conexão como execução (um passo).</summary>
public sealed class ConnectionTestPipeline : IRunPipeline
{
    public RunKind Kind => RunKind.ConnectionTest;

    public IReadOnlyList<Type> StepTypes { get; } = [typeof(TestConnectionStep)];
}
