using System.Text.Json;
using FluentAssertions;
using Mignow.Atc.Application.Modules.Report;

namespace Mignow.Atc.Tests.Report;

/// <summary>Paridade com o <c>coletar.py</c>: os números do <c>dados.json</c> gerado pelo script original sobre a mesma entrada.</summary>
public sealed class ReportDataGoldenTests
{
    private static readonly JsonElement Expected = ReportGoldenInput.Expected();
    private static readonly JsonElement Actual = JsonSerializer.SerializeToElement(ReportDataBuilder.Build(ReportGoldenInput.Load()));

    private static JsonElement At(JsonElement e, string path) => path.Split('.').Aggregate(e, (cur, p) => cur.GetProperty(p));

    /// <summary>Forma canônica: objetos com chaves em ordem, números e textos pelo valor (sem depender de escape ou indentação).</summary>
    internal static string Canon(JsonElement e) => e.ValueKind switch
    {
        JsonValueKind.Object => "{" + string.Join(",", e.EnumerateObject().OrderBy(p => p.Name, StringComparer.Ordinal).Select(p => p.Name + ":" + Canon(p.Value))) + "}",
        JsonValueKind.Array => "[" + string.Join(",", e.EnumerateArray().Select(Canon)) + "]",
        JsonValueKind.String => "\"" + e.GetString() + "\"",
        JsonValueKind.Number => e.GetDouble().ToString("R", System.Globalization.CultureInfo.InvariantCulture),
        _ => e.GetRawText(),
    };

    private static void Same(string path) => Canon(At(Actual, path)).Should().Be(Canon(At(Expected, path)), path);

    [Theory]
    [InlineData("objetos_total")]
    [InlineData("escopo")]
    [InlineData("extracao.serie")]
    [InlineData("extracao.total")]
    [InlineData("extracao.em_baseline")]
    [InlineData("extracao.com_baseline")]
    [InlineData("extracao.p1")]
    [InlineData("extracao.p2")]
    [InlineData("extracao.p3")]
    [InlineData("extracao.executada")]
    [InlineData("cliente.sid")]
    [InlineData("cliente.mandante")]
    [InlineData("cliente.request")]
    [InlineData("aplicacao.por_request")]
    [InlineData("aplicacao.fontes_ativadas")]
    [InlineData("aplicacao.manuais")]
    [InlineData("isencao.findings")]
    [InlineData("isencao.pedidos")]
    [InlineData("isencao.sem_escopo")]
    [InlineData("isencao.bloqueados")]
    [InlineData("isencao.erros")]
    [InlineData("isencao.motivos")]
    [InlineData("isencao.causas_sem_escopo")]
    [InlineData("isencao.aprovador")]
    [InlineData("custos.total.sessoes")]
    [InlineData("custos.total.liquido_s")]
    [InlineData("custos.total.total")]
    [InlineData("custos.total.saida")]
    [InlineData("custos.total.entrada_escrita")]
    [InlineData("custos.total.periodo")]
    [InlineData("custos.total.calendario_h")]
    [InlineData("custos.remediacao_liquido_s")]
    [InlineData("custos.liquido_por_finding_s")]
    [InlineData("custos.liquido_por_objeto_s")]
    [InlineData("comparacao_manual.premissas.objetos_dia")]
    [InlineData("comparacao_manual.premissas.horas_dia")]
    [InlineData("comparacao_manual.total")]
    [InlineData("comparacao_manual.ciclos")]
    public void Numeros_iguais_ao_script(string path) => Same(path);

    [Fact]
    public void Itens_e_ciclo_batem()
    {
        Actual.GetProperty("itens").GetArrayLength().Should().Be(Expected.GetProperty("itens").GetArrayLength());
        foreach (var k in new[] { "n", "ciclo", "obj", "tipo", "check", "msg", "ref", "sol", "sis", "req", "obs" })
            Actual.GetProperty("itens").EnumerateArray().Select(i => Canon(i.GetProperty(k)))
                .Should().Equal(Expected.GetProperty("itens").EnumerateArray().Select(i => Canon(i.GetProperty(k))), k);
        var a = Actual.GetProperty("ciclos")[0];
        var e = Expected.GetProperty("ciclos")[0];
        foreach (var k in new[] { "ciclo", "findings", "objetos", "por_solucao", "ja_tratados_antes", "escopo" })
            Canon(a.GetProperty(k)).Should().Be(Canon(e.GetProperty(k)), k);
    }

    [Fact]
    public void Fases_de_custo_na_mesma_ordem_com_o_mesmo_tempo()
    {
        var a = Actual.GetProperty("custos").GetProperty("fases").EnumerateArray().ToList();
        var e = Expected.GetProperty("custos").GetProperty("fases").EnumerateArray().ToList();
        a.Select(f => f.GetProperty("etapa").GetString()).Should().Equal(e.Select(f => f.GetProperty("etapa").GetString()));
        a.Select(f => f.GetProperty("liquido_s").GetInt64()).Should().Equal(e.Select(f => f.GetProperty("liquido_s").GetInt64()));
        a.Select(f => f.GetProperty("periodo").GetString()).Should().Equal(e.Select(f => f.GetProperty("periodo").GetString()));
    }

    [Fact]
    public void Burndown_tem_os_mesmos_pendentes_por_findings_e_objetos()
    {
        // A evolução do porte vem do histórico de status (lotes), não do historico.json: comparam-se as contagens, não os instantes.
        var a = Actual.GetProperty("evolucao").GetProperty("pontos").EnumerateArray().ToList();
        var e = Expected.GetProperty("evolucao").GetProperty("pontos").EnumerateArray().ToList();
        foreach (var k in new[] { "KB", "Análise", "Isenção", "Cancelado", "Pendente", "objetos_pendentes", "total", "total_objetos", "ciclo" })
            a.Select(p => Canon(p.GetProperty(k))).Should().Equal(e.Select(p => Canon(p.GetProperty(k))), k);
        a[0].GetProperty("rot").GetString().Should().Be(e[0].GetProperty("rot").GetString());
    }

    [Fact]
    public void Chaves_de_primeiro_nivel_iguais_as_do_dados_json()
    {
        Actual.EnumerateObject().Select(p => p.Name).Should().Equal(Expected.EnumerateObject().Select(p => p.Name));
    }
}
