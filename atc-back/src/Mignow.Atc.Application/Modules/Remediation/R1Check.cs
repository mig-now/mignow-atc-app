namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>
/// Conferência da R1 de um item (<c>conferir-r1</c>): <see cref="Proposal"/> = <c>Isenção</c> | <c>pendente</c> | <c>erro</c>.
/// <see cref="Reason"/> é o texto que o script grava (obs) quando aplica.
/// </summary>
public sealed record R1Check(int N, string ObjectName, string ObjectType, string Ref, string Note, string? Usage, string Proposal, string Reason)
{
    public const string Exemption = "Isenção";
    public const string Pending = "pendente";
    public const string Error = "erro";
}
