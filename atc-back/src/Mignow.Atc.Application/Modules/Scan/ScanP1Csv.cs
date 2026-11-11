using System.Globalization;
using Mignow.Atc.Domain.Scan;

namespace Mignow.Atc.Application.Modules.Scan;

/// <summary>
/// CSV P1 no layout da atc-remediacao (porte do trecho de <c>cruzar.py</c> que grava <c>mignow_&lt;SID&gt;_&lt;MANDT&gt;_&lt;data&gt;_remediacao.csv</c>):
/// colunas do layout + <c>Origem;Regra;Fonte ADT;Linha;Linha fim;Cruzamento;Trecho</c>. Só as classes MIGNOW_* (e ATC_OUTRA_LINHA
/// se pedido); prioridade da regra.
/// </summary>
public static class ScanP1Csv
{
    public static readonly IReadOnlyList<string> Layout =
    [
        "Priority", "Check Title", "Check Message", "Object name", "Obj.", "Exemption State", "Contact", "Package", "1st Found", "Obj. Resp.",
        "Changed by", "Note", "Short Text", "Component", "RObT", "Referenced Object", "Additional Info", "Category", "Category7", "Text", "Remarks",
        "Finding ID", "Origem", "Regra", "Fonte ADT", "Linha", "Linha fim", "Cruzamento", "Trecho",
    ];

    public static readonly IReadOnlyList<string> MignowClasses = [ScanCross.SourceWithoutFinding, ScanCross.SourceWithFinding];

    /// <summary>Ocorrências que vão para o CSV (mesma ordem da lista de entrada).</summary>
    public static IReadOnlyList<MignowOccurrence> Select(IEnumerable<MignowOccurrence> occurrences, bool includeOtherLine) =>
        [.. occurrences.Where(o => o.Cross is { } c && (MignowClasses.Contains(c) || includeOtherLine && c == ScanCross.AtcOtherLine))];

    public static string FileName(string sid, string client, DateTime stamp) =>
        $"mignow_{sid}_{client}_{stamp.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture)}_remediacao.csv";

    public static byte[] Write(
        IReadOnlyList<MignowOccurrence> selected, IReadOnlyDictionary<(string Type, string Name), ScanObject> scope, IReadOnlyDictionary<string, MignowRule> rules,
        DateTime now) => ScanCsv.Write(Layout, Rows(selected, scope, rules, now));

    /// <summary>Linhas do CSV P1 (na ordem do <see cref="Layout"/>).</summary>
    public static IReadOnlyList<IReadOnlyList<string>> Rows(
        IReadOnlyList<MignowOccurrence> selected, IReadOnlyDictionary<(string Type, string Name), ScanObject> scope, IReadOnlyDictionary<string, MignowRule> rules,
        DateTime now)
    {
        return [.. selected.Select(o =>
        {
            rules.TryGetValue(o.RuleId, out var r);
            scope.TryGetValue((o.ObjectType, o.ObjectName), out var e);
            var source = o.AdtSource ?? string.Empty;
            var lines = o.Line.ToString(CultureInfo.InvariantCulture) + (o.LineEnd is { } end && end != o.Line ? "-" + end.ToString(CultureInfo.InvariantCulture) : string.Empty);
            var v = new Dictionary<string, string>
            {
                ["Priority"] = r?.Priority ?? "1",
                ["Check Title"] = $"MIGNOW {o.RuleId} ({r?.Category ?? string.Empty})",
                ["Check Message"] = r?.Description ?? string.Empty,
                ["Object name"] = o.ObjectName,
                ["Obj."] = o.ObjectType,
                ["Package"] = e?.Package ?? string.Empty,
                ["1st Found"] = now.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
                ["Obj. Resp."] = e?.Author ?? string.Empty,
                ["RObT"] = r?.Robt ?? string.Empty,
                ["Referenced Object"] = o.Element,
                ["Additional Info"] = $"Varredura MIGNOW: {source} linha {lines}",
                ["Origem"] = "MIGNOW",
                ["Regra"] = o.RuleId,
                ["Fonte ADT"] = "/sap/bc/adt/" + source,
                ["Linha"] = o.Line.ToString(CultureInfo.InvariantCulture),
                ["Linha fim"] = (o.LineEnd ?? o.Line).ToString(CultureInfo.InvariantCulture),
                ["Cruzamento"] = o.Cross ?? string.Empty,
                ["Trecho"] = o.Snippet ?? string.Empty,
            };
            return (IReadOnlyList<string>)[.. Layout.Select(c => v.GetValueOrDefault(c, string.Empty))];
        })];
    }
}
