using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Registry;
using Mignow.Atc.Application.Ports.Persistence;

namespace Mignow.Atc.Application.Features.Settings.Get;

/// <summary>Aberto a todo usuário (o formulário das apps usa as tentativas padrão).</summary>
public sealed class GetSettingsHandler(IDataStore store) : IQueryHandler<GetSettingsQuery, Result<SettingsResponse>>
{
    public async Task<Result<SettingsResponse>> Handle(GetSettingsQuery query, CancellationToken ct) =>
        (await SettingsStore.LoadAsync(store, false, ct)).ToResponse();
}
