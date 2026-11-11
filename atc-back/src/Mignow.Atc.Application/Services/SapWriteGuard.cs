using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Application.Ports.Sap;

namespace Mignow.Atc.Application.Services;

/// <summary>
/// Regra da trava de escrita (porte de <c>Enable-AdtEscrita</c>): cadastro marcado como produção, mandante de produção
/// (T000-CCCATEGORY = P) ou categoria não confirmada ⇒ bloqueia. Os adapters leem a categoria e chamam esta regra.
/// </summary>
public static class SapWriteGuard
{
    public const string T000Sql = "SELECT cccategory FROM t000 WHERE mandt = '{0}'";

    public static Result<WriteGuardInfo> Evaluate(SapConnection connection, string? clientCategory, IStringLocalizer<Errors> l)
    {
        if (connection.IsProduction)
            return l.Forbidden("Sap.WriteBlockedProduction", connection.Client);
        if (string.IsNullOrWhiteSpace(clientCategory))
            return l.Forbidden("Sap.WriteBlockedCategoryUnknown", connection.Client);
        var category = clientCategory.Trim().ToUpperInvariant();
        if (category == "P")
            return l.Forbidden("Sap.WriteBlockedProductionClient", connection.Client);
        return new WriteGuardInfo(category);
    }

    public static string T000Query(string client) => string.Format(System.Globalization.CultureInfo.InvariantCulture, T000Sql, client.Replace("'", "''"));
}
