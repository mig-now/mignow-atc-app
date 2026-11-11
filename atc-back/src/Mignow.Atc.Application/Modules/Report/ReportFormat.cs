using System.Globalization;

namespace Mignow.Atc.Application.Modules.Report;

/// <summary>Formatadores do <c>gerar.py</c> (HTML escapado, milhar com ponto, horas e minutos, etc.), com o mesmo resultado do Python.</summary>
public static class ReportFormat
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary><c>html.escape</c> do Python (aspas e apóstrofo incluídos).</summary>
    public static string E(object? s) =>
        (s?.ToString() ?? "").Replace("&", "&amp;", StringComparison.Ordinal).Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal).Replace("\"", "&quot;", StringComparison.Ordinal).Replace("'", "&#x27;", StringComparison.Ordinal);

    /// <summary>Inteiro com milhar em ponto; nulo = "–".</summary>
    public static string N(long? v) => v is null ? "–" : v.Value.ToString("#,0", Inv).Replace(",", ".", StringComparison.Ordinal);

    public static string Mi(long v) =>
        (v / 1e6).ToString("#,0.0", Inv).Replace(",", "X", StringComparison.Ordinal).Replace(".", ",", StringComparison.Ordinal)
            .Replace("X", ".", StringComparison.Ordinal) + " mi";

    public static string Hm(long seconds)
    {
        var total = (long)Math.Round(seconds / 60.0, MidpointRounding.ToEven);
        var h = total / 60;
        var m = total % 60;
        return h > 0 ? $"{h} h {m:00} min" : $"{m} min";
    }

    public static string Pct(long a, long b) => b != 0 ? (a / (double)b * 100).ToString("0.00", Inv) + "%" : "0%";

    public static string Days(double v) =>
        v.ToString("0.0", Inv).Replace(".", ",", StringComparison.Ordinal) + (Math.Round(v, 1, MidpointRounding.ToEven) == 1 ? " dia" : " dias");

    public static string Sec(long s) => $"{s / 60} min {s % 60:00} s";

    public static string Factor(double a, double b) => b != 0 ? (a / b).ToString("0.0", Inv).Replace(".", ",", StringComparison.Ordinal) + "×" : "–";

    public static string Pill(object? text, string cls) => $"<span class=\"pill {cls}\">{E(text)}</span>";

    public static string Short(string model) => model.Replace("claude-", "", StringComparison.Ordinal);

    public static string Upper1(string s) => s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..];
}
