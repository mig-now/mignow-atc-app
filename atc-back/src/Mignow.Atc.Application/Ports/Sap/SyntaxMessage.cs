namespace Mignow.Atc.Application.Ports.Sap;

/// <summary>Mensagem de sintaxe/ativação. <see cref="Severity"/> E/W/I(/A/X). <see cref="Uri"/> = onde está (relativa ao ADT).</summary>
public sealed record SyntaxMessage(string Severity, string Text, int? Line, string? Uri);
