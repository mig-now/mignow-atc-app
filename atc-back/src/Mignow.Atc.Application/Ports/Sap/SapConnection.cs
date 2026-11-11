namespace Mignow.Atc.Application.Ports.Sap;

/// <summary>
/// Ambiente SAP de uma chamada (montado do cadastro + cofre pelo <c>SapConnections</c>). <c>sim://cenario</c> aponta
/// para o simulador. A senha existe só em memória: nunca em log nem em resposta. <see cref="IsProduction"/> vem do
/// cadastro e bloqueia toda escrita; <see cref="TrustOnFirstUse"/> só no teste de conexão (fixa o certificado).
/// </summary>
public sealed record SapConnection(
    Guid SystemId, string BaseUrl, string Client, string User, string Password, string Language, string? CertThumbprint, bool IsProduction,
    bool TrustOnFirstUse = false)
{
    public const string SimulatorScheme = "sim://";

    public bool IsSimulator => BaseUrl.StartsWith(SimulatorScheme, StringComparison.OrdinalIgnoreCase);

    public override string ToString() => $"{BaseUrl} ({Client}) {User}";
}
