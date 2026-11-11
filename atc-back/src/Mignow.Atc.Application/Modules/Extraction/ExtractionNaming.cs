using System.Text.RegularExpressions;

namespace Mignow.Atc.Application.Modules.Extraction;

/// <summary>
/// Nomes dos arquivos da skill: <c>atc_&lt;SID&gt;_&lt;MANDT&gt;_&lt;SERIE&gt;_&lt;AAAAMMDD-HHMMSS UTC&gt;_&lt;DISPLAY_ID&gt;</c>
/// (+ <c>.csv</c>, <c>_novos.csv</c>, <c>_remediacao.csv</c>); caracteres da série fora de <c>[A-Za-z0-9_-]</c> viram <c>_</c>.
/// </summary>
public static partial class ExtractionNaming
{
    public static string BaseName(string sid, string client, string series, DateTimeOffset? executedUtc, string displayId)
    {
        var stamp = executedUtc?.UtcDateTime.ToString("yyyyMMdd-HHmmss", System.Globalization.CultureInfo.InvariantCulture) ?? "00000000-000000";
        return $"atc_{sid}_{client}_{UnsafeRegex().Replace(series, "_")}_{stamp}_{displayId}";
    }

    public static string FileName(string baseName, ExtractionFileKind kind) => kind switch
    {
        ExtractionFileKind.Total => $"{baseName}.csv",
        ExtractionFileKind.New => $"{baseName}_novos.csv",
        _ => $"{baseName}_remediacao.csv",
    };

    [GeneratedRegex("[^A-Za-z0-9_-]")]
    private static partial Regex UnsafeRegex();
}
