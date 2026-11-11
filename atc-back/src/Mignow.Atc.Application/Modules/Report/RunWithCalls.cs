using Mignow.Atc.Domain.Execution;

namespace Mignow.Atc.Application.Modules.Report;

/// <summary>Uma execução do motor com as chamadas de IA registradas nela (insumo do histórico de processo).</summary>
public sealed record RunWithCalls(Run Run, IReadOnlyList<RunAiCall> AiCalls);
