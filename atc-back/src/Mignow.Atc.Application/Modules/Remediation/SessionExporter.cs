using Mignow.Atc.Application.Common;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>Arquivo exportado e o resumo por situação (<c>Pendente</c> para o status vazio).</summary>
public sealed record ExportedCsv(string FileName, byte[] Content, IReadOnlyDictionary<string, int> Summary);

/// <summary>
/// Porte de <c>cmd_exportar</c>: o CSV original (itens + linhas fora do escopo/em branco) com a coluna <c>Status Remediação</c> no fim (ou no lugar,
/// se o cabeçalho já tiver), mesma codificação e delimitador. O status das linhas sem item fica em branco. Acréscimo do porte: <see cref="ColApplication"/>.
/// </summary>
public static class SessionExporter
{
    public const string FileName = "atc_status.csv";

    private sealed record Excluded(int N, List<string> Row);

    /// <summary>
    /// Coluna do porte (não existe no <c>cmd_exportar</c>): resultado da última tentativa no SAP (<c>ATIVADO</c>, <c>ERRO_REQUEST: detalhe</c>…),
    /// porque "Confirmado" é só a decisão. Só entra quando algum item foi ao SAP (ou o CSV já tinha a coluna): sem aplicação, o arquivo é o da skill.
    /// </summary>
    public const string ColApplication = "Aplicação SAP";

    public static ExportedCsv Export(RemediationSession session, IReadOnlyList<RemediationItem> items)
    {
        var header = Json.Read(session.CsvColumnsJson, new List<string>());
        var status = items.ToDictionary(i => i.N, i => i.Status);
        var application = items.ToDictionary(i => i.N, ApplicationText);
        var rows = new SortedDictionary<int, IReadOnlyList<string>>();
        foreach (var e in Json.Read(session.ExcludedRowsJson, new List<Excluded>()))
            rows[e.N] = e.Row;
        foreach (var i in items)
            rows[i.N] = Json.Read(i.RawRowJson, new List<string>());

        var newHeader = header.ToList();
        var newRows = rows.ToDictionary(r => r.Key, r => r.Value.ToList());
        Put(newHeader, newRows, SessionImporter.ColStatus, n => status.GetValueOrDefault(n, string.Empty));
        if (header.Contains(ColApplication) || application.Values.Any(v => v.Length > 0))
            Put(newHeader, newRows, ColApplication, n => application.GetValueOrDefault(n, string.Empty));

        var summary = new Dictionary<string, int>();
        foreach (var v in status.Values)
            summary[v.Length == 0 ? "Pendente" : v] = summary.GetValueOrDefault(v.Length == 0 ? "Pendente" : v) + 1;
        return new ExportedCsv(FileName, AtcCsv.Write(session.CsvEncoding, session.CsvDelimiter, newHeader,
            newRows.Values.Select(r => (IReadOnlyList<string>)r).ToList()), summary);
    }

    /// <summary>Coluna no lugar (se o cabeçalho já tiver) ou no fim, como o <c>cmd_exportar</c> faz com o status.</summary>
    private static void Put(List<string> header, Dictionary<int, List<string>> rows, string column, Func<int, string> value)
    {
        var k = header.IndexOf(column);
        if (k < 0)
            header.Add(column);
        foreach (var (n, row) in rows)
        {
            if (k >= 0 && k < row.Count)
                row[k] = value(n);
            else
                row.Add(value(n));
        }
    }

    private static string ApplicationText(RemediationItem i)
    {
        if (i.ApplicationResult is null)
            return string.Empty;
        var detail = i.Application?.History.LastOrDefault()?.Detail;
        return Apply.ApplyResults.IsFailure(i.ApplicationResult) && !string.IsNullOrWhiteSpace(detail) ? $"{i.ApplicationResult}: {detail}" : i.ApplicationResult;
    }

    /// <summary>JSON das linhas que não viraram item, para <see cref="RemediationSession.SetExcludedRows"/>.</summary>
    public static string ExcludedJson(IEnumerable<(int N, IReadOnlyList<string> Row)> rows) =>
        Json.Write(rows.Select(r => new Excluded(r.N, [.. r.Row])).ToList());
}
