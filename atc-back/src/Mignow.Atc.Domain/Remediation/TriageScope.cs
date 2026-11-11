namespace Mignow.Atc.Domain.Remediation;

/// <summary>Para quais tipos de projeto a regra de triagem vale: todos, só upgrade ou só conversão.</summary>
public enum TriageScope
{
    All,
    Upgrade,
    Conversion,
}
