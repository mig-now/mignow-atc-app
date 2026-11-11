using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Exemption;

namespace Mignow.Atc.Application.Features.Exemptions;

/// <summary>Lote pelo id público (com os pedidos quando pedido).</summary>
public static class ExemptionBatchLoader
{
    public static async Task<Result<ExemptionBatch>> LoadAsync(IDataStore store, IStringLocalizer<Errors> l, Guid id, bool tracked, bool withRequests,
        CancellationToken ct)
    {
        var source = tracked ? store.Query<ExemptionBatch>() : store.Read<ExemptionBatch>();
        if (withRequests)
            source = store.Including(source, nameof(ExemptionBatch.Requests));
        var batch = await store.FirstOrDefaultAsync(source.Where(b => b.Guid == id), ct);
        return batch is null ? l.NotFound("Exemption.BatchNotFound") : batch;
    }
}
