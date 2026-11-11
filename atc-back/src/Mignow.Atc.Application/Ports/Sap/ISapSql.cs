using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Ports.Sap;

/// <summary>
/// SQL somente leitura pelo data preview do ADT (<c>POST /sap/bc/adt/datapreview/freestyle</c>, sempre STATELESS: o
/// stateful dá HTTP 500 depois de ~18 consultas). Porte de <c>Invoke-AdtSql</c>/<c>Invoke-AdtSqlIn</c>. Só SELECT.
/// </summary>
public interface ISapSql
{
    Task<Result<SqlResult>> SelectAsync(SapConnection connection, string sql, int maxRows, CancellationToken ct);

    /// <summary>SELECT com <c>IN (&lt;lista&gt;)</c> em blocos: o SQL contém o marcador <c>{IN}</c>.</summary>
    Task<Result<SqlResult>> SelectInAsync(SapConnection connection, string sql, IReadOnlyCollection<string> values, int blockSize, CancellationToken ct);
}
