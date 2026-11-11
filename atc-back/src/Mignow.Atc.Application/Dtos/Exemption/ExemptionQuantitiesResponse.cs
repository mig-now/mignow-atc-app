namespace Mignow.Atc.Application.Dtos.Exemption;

/// <summary>
/// Quantidades do lote (porte de <c>quantidades()</c> do <c>resultado.py</c>): a situação final de cada finding é a do último
/// passo (envio ou simulação). <c>Simulated</c> conta o que passou pela simulação (inclui o que já foi enviado).
/// </summary>
public sealed record ExemptionQuantitiesResponse(
    int Findings, int Simulated, int Requested, int Info, int Blocked, int NoScope, int Errors, int AlreadyRequested, int Pending);
