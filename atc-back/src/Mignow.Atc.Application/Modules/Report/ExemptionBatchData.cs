using Mignow.Atc.Domain.Exemption;

namespace Mignow.Atc.Application.Modules.Report;

/// <summary>Lote de isenção com os pedidos (equivale à pasta <c>isencoes/lote_*</c>); <c>At</c> = criação do lote.</summary>
public sealed record ExemptionBatchData(ExemptionBatch Batch, DateTimeOffset At, IReadOnlyList<ExemptionRequestData> Requests)
{
    public string Name => "lote_" + At.UtcDateTime.ToString("yyyyMMdd-HHmmss", System.Globalization.CultureInfo.InvariantCulture);
}
