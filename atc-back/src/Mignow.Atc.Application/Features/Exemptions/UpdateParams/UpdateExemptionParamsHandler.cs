using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Exemption;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Features.Exemptions.UpdateParams;

public sealed class UpdateExemptionParamsHandler(IDataStore store, IStringLocalizer<Errors> l)
    : ICommandHandler<UpdateExemptionParamsCommand, Result<ExemptionBatchResponse>>
{
    public async Task<Result<ExemptionBatchResponse>> Handle(UpdateExemptionParamsCommand command, CancellationToken ct)
    {
        var batch = await ExemptionBatchLoader.LoadAsync(store, l, command.Id, true, false, ct);
        if (batch.IsFailure)
            return batch.Error;
        var systemId = batch.Value.SapSystemId;
        var system = await store.FirstOrDefaultAsync(store.Read<SapSystem>().Where(s => s.Id == systemId), ct);
        // Princípio dos 4 olhos do SAP: o aprovador não pode ser quem pede (o usuário do ambiente).
        if (system is not null && !string.IsNullOrWhiteSpace(command.Approver) && ExemptionRules.IsRequester(command.Approver, system.Username))
            return l.Validation("Exemption.ApproverIsRequester", command.Approver.Trim().ToUpperInvariant());
        batch.Value.SetParameters(command.Approver, command.Reason, command.Validity, command.JustificationPrefix);
        return await ExemptionViews.BatchAsync(store, batch.Value, ct);
    }
}
