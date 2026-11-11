using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Sap;
using Mignow.Atc.Application.Ports.Secrets;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Application.Services;

/// <summary>Monta a conexão SAP do cadastro + cofre. A senha só existe em memória durante a chamada.</summary>
public sealed class SapConnections(ISecretStore secrets, IStringLocalizer<Errors> l)
{
    public async Task<Result<SapConnection>> ForAsync(SapSystem system, CancellationToken ct, bool trustOnFirstUse = false)
    {
        if (!system.Active)
            return l.Conflict("SapSystem.Inactive", system.DisplayName);
        var password = string.Empty;
        if (!system.IsSimulator)
        {
            if (system.PasswordSecretRef is null)
                return l.Conflict("SapSystem.PasswordMissing", system.DisplayName);
            password = await secrets.GetAsync(system.PasswordSecretRef, ct) ?? string.Empty;
            if (password.Length == 0)
                return l.Conflict("SapSystem.PasswordMissing", system.DisplayName);
        }
        return new SapConnection(system.Guid, system.BaseUrl, system.Client, system.Username, password, system.Language, system.CertThumbprint,
            system.IsProduction, trustOnFirstUse && system.CertThumbprint is null);
    }
}
