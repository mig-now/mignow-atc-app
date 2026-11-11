using FluentAssertions;
using Mignow.Atc.Application.Modules.Report;
using Mignow.Atc.Infra.External.Report;

namespace Mignow.Atc.Tests.Report;

public sealed class ReportHtmlSmokeTests
{
    private static readonly string[] Sections = ["s-ini", "s-evo", "s-res", "s-cmp", "s-sis", "s-isen", "s-kb", "s-pend", "s-lista", "s-proc", "s-cost"];
    private static readonly string[] Placeholders = ["{{TITULO}}", "<!--CORPO-->", "/*DADOS*/null"];

    [Theory]
    [InlineData("findings")]
    [InlineData("objetos")]
    public void Relatorio_tem_as_secoes_e_nenhum_placeholder_sobrando(string view)
    {
        var data = ReportDataBuilder.Build(ReportGoldenInput.Load());
        var html = ReportHtmlRenderer.Render(data, view, new EmbeddedReportTemplate().Html).Document;
        html.Should().StartWith("<!doctype html>");
        foreach (var id in Sections)
            html.Should().Contain($"id=\"{id}\"", id);
        foreach (var p in Placeholders)
            html.Should().NotContain(p);
        html.Should().Contain($"\"visao\":\"{view}\"").And.Contain("Remediação ATC").And.Contain("GOLD");
    }

    [Fact]
    public void Visao_por_objetos_conta_os_objetos()
    {
        var data = ReportDataBuilder.Build(ReportGoldenInput.Load());
        ReportHtmlRenderer.Render(data, "objetos", new EmbeddedReportTemplate().Html).Units.Should().Be(5);
        ReportHtmlRenderer.Render(data, "findings", new EmbeddedReportTemplate().Html).Units.Should().Be(6);
    }
}
