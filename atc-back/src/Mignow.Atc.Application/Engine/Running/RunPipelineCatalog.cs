using Microsoft.Extensions.DependencyInjection;
using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Engine.Running;

/// <summary>
/// Pipelines registrados, um por <see cref="RunKind"/> (duplicado = erro na partida). Os códigos dos passos saem das
/// instâncias dos passos (resolvidas num escopo), na ordem do pipeline.
/// </summary>
public sealed class RunPipelineCatalog
{
    private readonly Dictionary<RunKind, IRunPipeline> _pipelines;

    public RunPipelineCatalog(IEnumerable<IRunPipeline> pipelines)
    {
        _pipelines = [];
        foreach (var p in pipelines)
        {
            if (!_pipelines.TryAdd(p.Kind, p))
                throw new InvalidOperationException($"Mais de um pipeline para {p.Kind}: {_pipelines[p.Kind].GetType().Name} e {p.GetType().Name}.");
            if (p.StepTypes.Count == 0 || p.StepTypes.Any(t => !typeof(IRunStep).IsAssignableFrom(t)))
                throw new InvalidOperationException($"Pipeline {p.GetType().Name}: passos vazios ou que não são IRunStep.");
        }
    }

    public IReadOnlyCollection<RunKind> Kinds => _pipelines.Keys;

    public IRunPipeline? For(RunKind kind) => _pipelines.GetValueOrDefault(kind);

    public IReadOnlyList<string> StepCodes(RunKind kind, IServiceProvider scope) =>
        For(kind) is { } p ? [.. p.StepTypes.Select(t => ((IRunStep)scope.GetRequiredService(t)).Code)] : [];
}
