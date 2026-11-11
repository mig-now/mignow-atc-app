using System.Globalization;
using Mignow.Atc.Application.Ports.Sap.Extraction;

namespace Mignow.Atc.Application.Modules.Extraction;

/// <summary>
/// Escolha da execução do ATC (porte do trecho "execuções da série" do <c>extrair.ps1</c>): execução = cabeçalho com
/// <c>DISPLAY_ID = EXEC_ID</c> e sem <c>IS_REWORKED</c> (reavaliações/baselines ficam de fora), série com prefixo EXATO
/// (<c>ZMIG_</c> ou <c>Z_MIG_</c>; o <c>_</c> é curinga no LIKE, por isso o filtro é refeito aqui). A última é a mais nova
/// com <c>IS_COMPLETE = X</c>; as mais novas ainda em andamento só são informadas.
/// </summary>
public static class AtcRunSelector
{
    public static readonly IReadOnlyList<string> DefaultPrefixes = ["ZMIG_", "Z_MIG_"];

    public static AtcRunSelection Select(IEnumerable<AtcRunHeader> headers, IReadOnlyList<string> prefixes)
    {
        var executions = headers
            .Where(h => prefixes.Any(p => h.Series.StartsWith(p, StringComparison.Ordinal))
                        && string.Equals(h.DisplayId, h.ExecId, StringComparison.OrdinalIgnoreCase) && !h.IsReworked)
            .OrderByDescending(h => Timestamp(h.ScheduledOnTs))
            .ToList();
        var latest = executions.FirstOrDefault(h => h.IsComplete);
        if (latest is null)
            return new AtcRunSelection(null, [], executions.Count);
        var inProgress = executions.Where(h => !h.IsComplete && Timestamp(h.ScheduledOnTs) > Timestamp(latest.ScheduledOnTs)).ToList();
        return new AtcRunSelection(latest, inProgress, executions.Count);
    }

    /// <summary><c>SCHEDULED_ON_TS</c> como decimal (a ordenação do script é numérica).</summary>
    public static decimal Timestamp(string ts) =>
        decimal.TryParse(ts, NumberStyles.Number, CultureInfo.InvariantCulture, out var v) ? v : 0m;

    /// <summary>Data/hora UTC da execução a partir do <c>SCHEDULED_ON_TS</c> (<c>AAAAMMDDHHMMSS[.f]</c>); nula se ilegível.</summary>
    public static DateTimeOffset? ParseUtc(string ts)
    {
        var s = ts.Trim();
        if (s.Length < 14 || !s[..14].All(char.IsAsciiDigit))
            return null;
        return DateTimeOffset.TryParseExact(s[..14], "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out var d)
            ? d
            : null;
    }
}
