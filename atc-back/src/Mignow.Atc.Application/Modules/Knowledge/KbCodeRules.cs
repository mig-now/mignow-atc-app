using System.Text.RegularExpressions;

namespace Mignow.Atc.Application.Modules.Knowledge;

/// <summary>Porte de <c>alertas</c>, <c>n_comandos</c> e <c>complexidade_card</c> do atc.py (textos idênticos).</summary>
public static partial class KbCodeRules
{
    private const RegexOptions I = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(2);

    private const string ConversionPrefix = "Conversão ALPHA";

    private static readonly (Regex Rx, string Message)[] Rules =
    [
        (new Regex(@"\+18\s*\(\s*0?4\s*\)|CONCATENATE\s+\S*\(18\)\s+\S+\s+\S+\s+INTO\s+\S+\s+RESPECTING\s+BLANKS|WIDTH\s*=\s*18[^}]*\}\s*\{", I, Timeout),
            "Chave de lote montada com material em 18 posições: no S/4 a chave é MATNR(40)+WERKS+CHARG "
            + "(falha em CLAF/OBJECTKEY_LONG/SELECT direto). Use o layout de 40 posições e OBJECTKEY_LONG."),
        (new Regex(@"(MATNR|MATERIAL)\w*\)?\(18\)", I, Timeout),
            "Material truncado em 18 posições: as BAPIs têm parâmetro MATERIAL_LONG; use o MATNR completo."),
        (new Regex(@"CONVERSION_EXIT_ALPHA_(INPUT|OUTPUT)", I, Timeout),
            ConversionPrefix + " aplicada a material: use CONVERSION_EXIT_MATN1_INPUT/OUTPUT."),
    ];

    private const string LoopAlert = "SELECT ... INTO TABLE dentro de LOOP sobrescreve o resultado a cada volta: use FOR ALL ENTRIES ou APPENDING.";

    [GeneratedRegex(@"CALL\s+TRANSACTION\s+'(\w+N)'\s+USING", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex BdcN();

    [GeneratedRegex(@"\bALPHA\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex Alpha();

    [GeneratedRegex("MAT", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex Mat();

    [GeneratedRegex(@"LOOP\s+AT[\s\S]*?SELECT[\s\S]*?INTO\s+(CORRESPONDING\s+FIELDS\s+OF\s+)?TABLE[\s\S]*?ENDLOOP", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 2000)]
    private static partial Regex LoopSelect();

    [GeneratedRegex(@"'[^']*'|`[^`]*`")]
    private static partial Regex Literals();

    public static List<string> Alerts(string code, string original = "")
    {
        var res = new List<string>();
        foreach (var (rx, msg) in Rules)
            if (Safe(() => rx.IsMatch(code)))
                res.Add(msg);
        var m = BdcN().Match(code);
        if (m.Success && (original.Length == 0 || Regex.IsMatch(original, "'" + Regex.Escape(m.Groups[1].Value[..^1]) + "'", I)))
        {
            res.Add($"Troca para {m.Groups[1].Value.ToUpperInvariant()} mantendo batch input (USING bdcdata): a transação Enjoy usa outro "
                    + "programa/telas e o BDC da transação antiga provavelmente não funciona. Prefira a BAPI equivalente "
                    + "(ex.: BAPI_PR_CHANGE/BAPI_REQUISITION_DELETE para ME52N, BAPI_OBJCL_CHANGE para MSC2N) "
                    + "ou adapte as telas do bdcdata.");
        }
        if (Alpha().IsMatch(code) && !Mat().IsMatch(code))
            res = [.. res.Where(a => !a.StartsWith(ConversionPrefix, StringComparison.Ordinal))];
        var up = code.ToUpperInvariant();
        if (Safe(() => LoopSelect().IsMatch(up)) && !up.Contains("APPENDING", StringComparison.Ordinal))
            res.Add(LoopAlert);
        return res;
    }

    /// <summary>Número aproximado de comandos ABAP: pontos finais fora de comentário/literal (mínimo 1 se há código).</summary>
    public static int CommandCount(string code)
    {
        var n = 0;
        foreach (var line in KbText.SplitLines(code))
        {
            var l = line.TrimStart();
            if (l.StartsWith('*') || l.StartsWith('"'))
                continue;
            var cut = line.Split('"', 2)[0];
            n += Literals().Replace(cut, "").Count(c => c == '.');
        }
        return string.IsNullOrWhiteSpace(code) ? 0 : Math.Max(n, 1);
    }

    /// <summary>
    /// simples (até 3 comandos de cada lado, sem alerta) | media (adaptação de modelo existente) | complexa.
    /// Como no <c>kb_lookup</c>, a marca de substituída só entra depois do cálculo: aqui vale apenas o alerta do código.
    /// </summary>
    public static string Complexity(IReadOnlyCollection<string> alerts, string original, string @new)
    {
        if (alerts.Count > 0)
            return "complexa";
        var o = CommandCount(original);
        var n = CommandCount(@new);
        if (o <= 3 && n <= 3)
            return "simples";
        return n <= 15 ? "media" : "complexa";
    }

    private static bool Safe(Func<bool> f)
    {
        try
        {
            return f();
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }
}
