namespace Mignow.Atc.Application.Ports.Sap.Ddic;

/// <summary>Validação do nome/pacote na criação (<c>Test-AdtDdicName</c>): <see cref="Text"/> = mensagem do SAP.</summary>
public sealed record DdicNameValidation(bool Ok, string Severity, string Text);
