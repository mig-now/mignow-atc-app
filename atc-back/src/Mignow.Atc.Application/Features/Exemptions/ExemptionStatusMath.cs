using Mignow.Atc.Application.Dtos.Exemption;
using Mignow.Atc.Domain.Exemption;

namespace Mignow.Atc.Application.Features.Exemptions;

/// <summary>Contagens por status e quantidades do lote (porte do <c>quantidades()</c> do <c>resultado.py</c>).</summary>
public static class ExemptionStatusMath
{
    public const string PendingKey = "PENDENTE";

    public static IReadOnlyDictionary<string, int> Totals(IEnumerable<(string? Status, bool HasRequestXml)> requests) =>
        requests.GroupBy(r => r.Status ?? PendingKey).OrderBy(g => g.Key, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Count());

    public static ExemptionQuantitiesResponse Quantities(IReadOnlyCollection<(string? Status, bool HasRequestXml)> requests)
    {
        int Count(string status) => requests.Count(r => r.Status == status);
        // Simulado = passou pela simulação: ainda SIMULADO ou já enviado (o XML do pedido só existe depois de simular com sucesso).
        var simulated = requests.Count(r => r.Status == ExemptionRequestStatus.Simulated
                                            || r.HasRequestXml && r.Status is ExemptionRequestStatus.Requested or ExemptionRequestStatus.Info or ExemptionRequestStatus.Error);
        return new ExemptionQuantitiesResponse(requests.Count, simulated, Count(ExemptionRequestStatus.Requested), Count(ExemptionRequestStatus.Info),
            Count(ExemptionRequestStatus.Blocked), Count(ExemptionRequestStatus.NoScope), Count(ExemptionRequestStatus.Error),
            Count(ExemptionRequestStatus.AlreadyRequested), requests.Count(r => r.Status is null));
    }
}
