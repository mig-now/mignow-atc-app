using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Engine.Running;

/// <summary>
/// Pipeline de um <see cref="RunKind"/> (um por tipo). Registrado por varredura de assembly: um módulo novo só cria a
/// classe (sealed, construtor sem parâmetros) — nada de editar arquivo compartilhado.
/// </summary>
public interface IRunPipeline
{
    RunKind Kind { get; }

    /// <summary>Tipos dos passos (<see cref="IRunStep"/>), na ordem; resolvidos no escopo da execução.</summary>
    IReadOnlyList<Type> StepTypes { get; }
}
