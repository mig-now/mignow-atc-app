namespace Mignow.Atc.Application.Ports.Sap;

/// <summary>Ativação: <see cref="Error"/> = HTTP de erro ou mensagem E/A/X; mensagens como <c>"E: texto"</c>.</summary>
public sealed record ActivationResult(int Status, bool Error, IReadOnlyList<SyntaxMessage> Messages);
