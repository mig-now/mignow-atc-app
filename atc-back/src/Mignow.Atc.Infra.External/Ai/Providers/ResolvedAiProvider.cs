using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Infra.External.Ai.Providers;

/// <summary>Provedor padrão (único para todos os usos), já com a chave do cofre (só em memória, nunca em log).</summary>
public sealed record ResolvedAiProvider(AiProviderKind Kind, string Model, string? ApiKey);
