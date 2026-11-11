using System.Text.RegularExpressions;
using System.Xml.Linq;
using Mignow.Atc.Application.Ports.Sap.Extraction;

namespace Mignow.Atc.Infra.External.Sap.Adt;

/// <summary>
/// Leitura do XML de <c>/sap/bc/adt/atc/results/{displayId}</c> (porte da seção 2 do <c>extrair.ps1</c>): cada
/// <c>atcobject:object</c> dá colunas <c>obj_&lt;atributo&gt;</c>; cada <c>atcfinding:finding</c> dá os atributos, <c>item_id</c>
/// (do <c>uri</c>, <c>itemid/&lt;hex&gt;</c>), <c>documentation_link</c>, <c>quickfix_*</c> e <c>tag_&lt;nome&gt;</c>.
/// Nomes lidos pelo nome local (o prefixo varia). Colunas de tag em ordem alfabética (cultura invariante).
/// </summary>
internal static partial class AdtAtcXml
{
    public const string DocumentationRel = "http://www.sap.com/adt/relations/documentation";

    public static AtcRawFindings ParseFindings(string xml)
    {
        var doc = XDocument.Parse(xml, LoadOptions.None);
        var rows = new List<IReadOnlyDictionary<string, string>>();
        var objectColumns = new List<string>();
        var findingColumns = new List<string>();
        var tagColumns = new SortedSet<string>(StringComparer.InvariantCulture);

        foreach (var o in doc.Descendants().Where(e => e.Name.LocalName == "object" && e.Name.NamespaceName.EndsWith("/atc/object", StringComparison.Ordinal)))
        {
            var objectAttributes = new List<(string Column, string Value)>();
            foreach (var a in Attrs(o))
            {
                var column = "obj_" + a.Name.LocalName;
                objectAttributes.Add((column, a.Value));
                AddColumn(objectColumns, column);
            }
            var findings = o.Elements().Where(e => e.Name.LocalName == "findings").SelectMany(f => f.Elements().Where(x => x.Name.LocalName == "finding"));
            foreach (var f in findings)
            {
                var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var (column, value) in objectAttributes)
                    row[column] = value;
                foreach (var a in Attrs(f))
                {
                    row[a.Name.LocalName] = a.Value;
                    AddColumn(findingColumns, a.Name.LocalName);
                }
                row["item_id"] = ItemIdRegex().Match(row.GetValueOrDefault("uri") ?? string.Empty).Groups[1].Value.ToUpperInvariant();
                var doc2 = f.Elements().FirstOrDefault(e => e.Name.LocalName == "link" && (string?)e.Attribute("rel") == DocumentationRel);
                if (doc2 is not null)
                    row["documentation_link"] = (string?)doc2.Attribute("href") ?? string.Empty;
                var quickfix = f.Elements().FirstOrDefault(e => e.Name.LocalName == "quickfixes");
                if (quickfix is not null)
                    foreach (var a in Attrs(quickfix))
                    {
                        var column = "quickfix_" + a.Name.LocalName;
                        row[column] = a.Value;
                        AddColumn(findingColumns, column);
                    }
                var tags = f.Elements().Where(e => e.Name.LocalName == "tags").SelectMany(t => t.Elements().Where(x => x.Name.LocalName == "tag"));
                foreach (var t in tags)
                {
                    var column = "tag_" + (t.Attributes().FirstOrDefault(a => a.Name.LocalName == "name")?.Value ?? string.Empty);
                    row[column] = t.Attributes().FirstOrDefault(a => a.Name.LocalName == "value")?.Value ?? string.Empty;
                    tagColumns.Add(column);
                }
                rows.Add(row);
            }
        }
        return new AtcRawFindings(objectColumns, findingColumns, [.. tagColumns], rows);
    }

    private static IEnumerable<XAttribute> Attrs(XElement e) => e.Attributes().Where(a => !a.IsNamespaceDeclaration);

    private static void AddColumn(List<string> list, string name)
    {
        if (!list.Contains(name))
            list.Add(name);
    }

    [GeneratedRegex("itemid/([0-9A-Fa-f]+)")]
    private static partial Regex ItemIdRegex();
}
