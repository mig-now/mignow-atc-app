using System.Text.RegularExpressions;

namespace Mignow.Atc.Application.Modules.Remediation.Apply;

/// <summary>
/// Alertas sobre o código novo (porte de <c>alertas()</c>/<c>ALERTAS</c>/<c>ALERTA_LOOP</c>/<c>ALERTA_BDC_N</c> do atc.py): padrões que a
/// análise das remediações (docs/ANALISE_REMEDIACOES.md) mostrou serem problemáticos. Valem para a solução da KB e para a da IA.
/// </summary>
public static partial class ProposalAlerts
{
    public const string LoopAlert = "SELECT ... INTO TABLE dentro de LOOP sobrescreve o resultado a cada volta: use FOR ALL ENTRIES ou APPENDING.";

    private static readonly (Regex Pattern, string Message)[] Fixed =
    [
        (BatchKey18(),
            "Chave de lote montada com material em 18 posições: no S/4 a chave é MATNR(40)+WERKS+CHARG "
            + "(falha em CLAF/OBJECTKEY_LONG/SELECT direto). Use o layout de 40 posições e OBJECTKEY_LONG."),
        (Material18(), "Material truncado em 18 posições: as BAPIs têm parâmetro MATERIAL_LONG; use o MATNR completo."),
        (AlphaConversion(), "Conversão ALPHA aplicada a material: use CONVERSION_EXIT_MATN1_INPUT/OUTPUT."),
    ];

    /// <summary>Alertas do <paramref name="code"/> (código novo); <paramref name="original"/> = código atual, para a troca de transação Enjoy.</summary>
    public static IReadOnlyList<string> Of(string code, string original = "")
    {
        List<string> res = [.. Fixed.Where(f => f.Pattern.IsMatch(code)).Select(f => f.Message)];
        var m = BdcEnjoy().Match(code);
        if (m.Success && (original.Length == 0 || Regex.IsMatch(original, "'" + Regex.Escape(m.Groups[1].Value[..^1]) + "'", RegexOptions.IgnoreCase)))
            res.Add($"Troca para {m.Groups[1].Value.ToUpperInvariant()} mantendo batch input (USING bdcdata): a transação Enjoy usa outro "
                    + "programa/telas e o BDC da transação antiga provavelmente não funciona. Prefira a BAPI equivalente "
                    + "(ex.: BAPI_PR_CHANGE/BAPI_REQUISITION_DELETE para ME52N, BAPI_OBJCL_CHANGE para MSC2N) "
                    + "ou adapte as telas do bdcdata.");
        if (AlphaWord().IsMatch(code) && !MatText().IsMatch(code))
            res = [.. res.Where(a => !a.StartsWith("Conversão ALPHA", StringComparison.Ordinal))];
        var up = code.ToUpperInvariant();
        if (LoopSelect().IsMatch(up) && !up.Contains("APPENDING", StringComparison.Ordinal))
            res.Add(LoopAlert);
        return res;
    }

    [GeneratedRegex(@"\+18\s*\(\s*0?4\s*\)|CONCATENATE\s+\S*\(18\)\s+\S+\s+\S+\s+INTO\s+\S+\s+RESPECTING\s+BLANKS|WIDTH\s*=\s*18[^}]*\}\s*\{", RegexOptions.IgnoreCase)]
    private static partial Regex BatchKey18();

    [GeneratedRegex(@"(MATNR|MATERIAL)\w*\)?\(18\)", RegexOptions.IgnoreCase)]
    private static partial Regex Material18();

    [GeneratedRegex(@"CONVERSION_EXIT_ALPHA_(INPUT|OUTPUT)", RegexOptions.IgnoreCase)]
    private static partial Regex AlphaConversion();

    [GeneratedRegex(@"CALL\s+TRANSACTION\s+'(\w+N)'\s+USING", RegexOptions.IgnoreCase)]
    private static partial Regex BdcEnjoy();

    [GeneratedRegex(@"\bALPHA\b", RegexOptions.IgnoreCase)]
    private static partial Regex AlphaWord();

    [GeneratedRegex("MAT", RegexOptions.IgnoreCase)]
    private static partial Regex MatText();

    [GeneratedRegex(@"LOOP\s+AT[\s\S]*?SELECT[\s\S]*?INTO\s+(CORRESPONDING\s+FIELDS\s+OF\s+)?TABLE[\s\S]*?ENDLOOP")]
    private static partial Regex LoopSelect();
}
