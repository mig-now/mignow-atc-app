using System.Globalization;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Ports.Ai;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Infra.External.Ai.Fake;

namespace Mignow.Atc.Tests.Integration;

[Collection(IntegrationCollection.Name)]
[Trait("Category", "Integration")]
public sealed class AiAndTextTests(AtcApiFactory api)
{
    private sealed record Echo(string Text);

    private static readonly System.Text.Json.JsonElement Schema = System.Text.Json.JsonDocument.Parse("{\"type\":\"object\"}").RootElement;

    [Fact]
    public async Task Ia_fake_responde_pelo_proposito_e_registra_o_consumo()
    {
        var gateway = api.Services.GetRequiredService<IAiGateway>();
        var calls = new List<AiUsageInfo>();
        var result = await gateway.CompleteAsync<Echo>(new AiRequest(AiUsage.Mechanical, EchoFakeResponder.Purpose, "sys", "olá", Schema), calls.Add,
            CancellationToken.None);
        result.Value.Text.Should().Be("olá");
        calls.Should().ContainSingle().Which.Model.Should().Be("fake");
    }

    [Fact]
    public async Task Proposito_sem_resposta_fake_nao_esta_configurado()
    {
        var gateway = api.Services.GetRequiredService<IAiGateway>();
        var result = await gateway.CompleteAsync<Echo>(new AiRequest(AiUsage.Analysis, "nao.existe", "s", "u", Schema), _ => { }, CancellationToken.None);
        result.Error.Code.Should().Be("Ai.NotConfigured");
    }

    [Fact]
    public void ErrorText_procura_o_codigo_nos_recursos_e_traduz()
    {
        var l = api.Services.GetRequiredService<IStringLocalizer<Errors>>();
        l.Should().BeOfType<ErrorText>();
        ErrorText.ResourceTypes.Select(t => t.Name).Should().Contain(["Errors", "ExtractionErrors", "RemediationErrors", "ExemptionErrors", "ScanErrors",
            "ReportErrors", "KnowledgeErrors"]);
        var previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");
            l["Run.NotFound"].Value.Should().Be("Execução não encontrada.");
            CultureInfo.CurrentUICulture = new CultureInfo("es-ES");
            l["Run.NotFound"].Value.Should().Be("Ejecución no encontrada.");
            l["Codigo.Inexistente"].ResourceNotFound.Should().BeTrue();
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }
}
