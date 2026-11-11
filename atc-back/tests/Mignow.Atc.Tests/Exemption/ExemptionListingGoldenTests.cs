using System.Text.Json;
using FluentAssertions;
using Mignow.Atc.Application.Features.Exemptions;

namespace Mignow.Atc.Tests.Exemption;

/// <summary>Golden do <c>listar.py</c> (saída do script Python original sobre entradas sintéticas; ver <c>Golden/exemption/gerar.sh</c>).</summary>
public sealed class ExemptionListingGoldenTests
{
    private static string Path(string file) => System.IO.Path.Combine(AppContext.BaseDirectory, "Golden", "exemption", file);

    private static JsonElement Json(string file) => JsonDocument.Parse(File.ReadAllText(Path(file))).RootElement;

    private static string S(JsonElement e, string name) => e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString()! : string.Empty;

    private static List<ExemptionListingItem> Items() =>
        [.. Json("sessao.json").GetProperty("itens").EnumerateArray().Select(i => new ExemptionListingItem(
            i.GetProperty("n").GetInt32(), i.GetProperty("n").GetInt32(), S(i, "objeto"), S(i, "tipo"), S(i, "ref"), S(i, "check"), S(i, "msg"), S(i, "obs"),
            S(i, "status"), S(i, "prioridade") is { Length: > 0 } p ? p : null, S(i, "finding_id") is { Length: > 0 } f ? f : null))];

    private static List<ExemptionListingFinding> Extraction()
    {
        var lines = File.ReadAllLines(Path("novos.csv")).Select(l => l.TrimStart('﻿')).ToList();
        var header = lines[0].Split(';');
        return [.. lines.Skip(1).Select(l =>
        {
            var c = l.Split(';');
            string V(string name) => c[Array.IndexOf(header, name)];
            return new ExemptionListingFinding(V("quickfixInfo"), V("obj_name"), V("obj_type"), V("tag_REF_OBJ_NAME"), V("messageTitle"), V("priority"),
                V("checksum"), V("location"), V("exemptionApproval"));
        })];
    }

    private static HashSet<string> AlreadyRequested() =>
        [.. Json("isencoes.json").EnumerateObject().Where(p => p.Value.GetProperty("tipo").GetString() == "S").Select(p => p.Name)];

    private static ExemptionListingResult Run(params string[] priorities) =>
        ExemptionListing.Build(Items(), Extraction(), AlreadyRequested(), priorities, true, null, null);

    [Theory]
    [InlineData("esperado_123.json", "lista_123.json", "1,2,3")]
    [InlineData("esperado_1.json", "lista_1.json", "1")]
    public void Coincide_com_o_listar_py(string expectedFile, string listFile, string priorities)
    {
        var expected = Json(expectedFile);
        var expectedList = Json(listFile).GetProperty("findings").EnumerateArray().ToList();
        var result = Run(priorities.Split(','));
        var r = result.Report;

        r.SelectedItems.Should().Be(expected.GetProperty("itens_selecionados").GetInt32());
        r.Findings.Should().Be(expected.GetProperty("findings").GetInt32());
        r.OutOfScope.Should().Be(expected.GetProperty("fora_do_escopo").GetInt32());
        r.WithoutPriority.Select(x => x.ItemNumber).Should().Equal(expected.GetProperty("sem_prioridade").EnumerateArray().Select(x => x.GetProperty("n").GetInt32()));
        r.WithoutLink.Select(x => x.ItemNumber).Should().Equal(expected.GetProperty("sem_vinculo").EnumerateArray().Select(x => x.GetProperty("n").GetInt32()));
        r.AlreadyExempt.Select(x => x.MarkerId).Should().Equal(expected.GetProperty("ja_isentos_na_extracao").EnumerateArray().Select(x => x.GetProperty("marker_id").GetString()));
        r.AlreadyRequested.Select(x => x.MarkerId).Should().Equal(expected.GetProperty("ja_pedidos_pela_skill").EnumerateArray().Select(x => x.GetProperty("marker_id").GetString()));
        r.DivergentGroups.Select(g => (g.ObjectName, g.Findings, string.Join(',', g.ItemNumbers))).Should().Equal(
            expected.GetProperty("grupos_itens_x_findings_divergentes").EnumerateArray()
                .Select(g => (g.GetProperty("objeto").GetString()!, g.GetProperty("findings").GetInt32(), string.Join(',', g.GetProperty("itens").EnumerateArray().Select(x => x.GetInt32())))));
        r.ByOrigin["finding_id"].Should().Be(expected.GetProperty("por_origem").GetProperty("finding_id").GetInt32());
        r.ByOrigin["cruzamento"].Should().Be(expected.GetProperty("por_origem").GetProperty("cruzamento").GetInt32());
        r.ProbablyNoScope.Select(x => (x.ItemNumber, x.Detail)).Should().Equal(
            expected.GetProperty("provavel_sem_escopo").EnumerateArray().Select(x => (x.GetProperty("n").GetInt32(), (string?)x.GetProperty("aviso_escopo").GetString())));
        r.Sample.Select(x => (x.Seq, x.ItemNumber, x.Include, x.Line)).Should().Equal(
            expected.GetProperty("amostra").EnumerateArray().Select(x => (x.GetProperty("seq").GetInt32(), x.GetProperty("n").GetInt32(),
                x.GetProperty("include").GetString() is { Length: > 0 } inc ? inc : null, x.GetProperty("linha").ValueKind == JsonValueKind.Number ? x.GetProperty("linha").GetInt32() : (int?)null)));

        result.Entries.Should().HaveCount(expectedList.Count);
        for (var i = 0; i < expectedList.Count; i++)
        {
            var e = expectedList[i];
            var a = result.Entries[i];
            a.Seq.Should().Be(e.GetProperty("seq").GetInt32());
            a.Item.N.Should().Be(e.GetProperty("n").GetInt32());
            a.MarkerId.Should().Be(e.GetProperty("marker_id").GetString());
            a.Origin.Should().Be(e.GetProperty("origem").GetString());
            a.ScopeWarning.Should().Be(e.GetProperty("aviso_escopo").GetString());
            (a.Include ?? string.Empty).Should().Be(e.GetProperty("include").GetString());
            a.Line.Should().Be(e.GetProperty("linha").ValueKind == JsonValueKind.Number ? e.GetProperty("linha").GetInt32() : null);
            a.GroupItemNumbers.Should().Equal(e.GetProperty("itens_do_grupo").EnumerateArray().Select(x => x.GetInt32()));
        }
    }
}
