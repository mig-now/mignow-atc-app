namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>Regra de triagem por produto MIGNOW (<c>produtos_mignow.json</c>): objetos do produto têm o finding ignorado (Cancelado).</summary>
public sealed record ProductRule(string Name, string Description, IReadOnlySet<string> Objects);
