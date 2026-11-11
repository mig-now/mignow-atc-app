using System.Text.Json;
using Mignow.Atc.Application.Modules.Remediation;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Tests.Remediation;

/// <summary>Entradas sintéticas do golden da remediação (<c>Golden/remediation/entrada</c>) e a saída esperada do atc.py original (<c>esperado</c>).</summary>
internal static class GoldenFixture
{
    public static readonly string Dir = Path.Combine(AppContext.BaseDirectory, "Golden", "remediation");

    public static byte[] InputCsv => File.ReadAllBytes(Path.Combine(Dir, "entrada", "atc.csv"));

    public static JsonElement Expected(string file) => JsonDocument.Parse(File.ReadAllText(Path.Combine(Dir, "esperado", file))).RootElement;

    public static (CsvDocument Doc, ImportOutcome Outcome, List<RemediationItem> Items) Import()
    {
        var doc = AtcCsv.Read(InputCsv)!;
        var outcome = SessionImporter.Import(doc, "1,2");
        var items = outcome.Rows.Select(r => RemediationItem.Create(1, r.N, r.ObjectName, r.ObjectType, r.Ref, r.RefType, r.Check, r.Message, r.Note, r.Text,
            r.Package, r.Classification, r.Priority, r.FindingId, ItemOrigin.Atc, Mignow.Atc.Application.Common.Json.Write(r.RawRow))).ToList();
        return (doc, outcome, items);
    }

    public static SourceIndex Sources()
    {
        var root = Path.Combine(Dir, "entrada", "fontes");
        return new SourceIndex(Directory.GetDirectories(root).SelectMany(d => Directory.GetFiles(d, "*.abap")
            .Select(f => new SourceFileText(Path.GetFileName(d), Path.GetFileNameWithoutExtension(f), File.ReadAllText(f)))));
    }

    public static TriageResult Triage(IReadOnlyList<RemediationItem> items, ProjectType type) =>
        TriageEngine.Classify(items, Sources(), type, new DefaultProductRules().Get());
}
