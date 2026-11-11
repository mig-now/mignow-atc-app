using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Mignow.Atc.Tests.Integration;

namespace Mignow.Atc.Tests.Exemption;

/// <summary>Rotas de isenção pela API (banco de teste): erros de contrato. O fluxo completo roda no <see cref="ExemptionPipelineTests"/>.</summary>
[Collection(IntegrationCollection.Name)]
[Trait("Category", "Integration")]
public sealed class ExemptionApiTests(AtcApiFactory api)
{
    [Fact]
    public async Task Lote_inexistente_responde_404_com_codigo_estavel()
    {
        var client = api.CreateClient();
        var id = Guid.NewGuid();
        foreach (var response in new[]
                 {
                     await client.GetAsync($"/v1/exemption-batches/{id}"),
                     await client.GetAsync($"/v1/exemption-batches/{id}/requests"),
                     await client.PostAsync($"/v1/exemption-batches/{id}/discover", null),
                     await client.PostAsync($"/v1/exemption-batches/{id}/check", null),
                 })
        {
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            (await response.ProblemCodeAsync()).Should().Be("Exemption.BatchNotFound");
        }
    }

    [Fact]
    public async Task Criar_lote_valida_prioridades_e_sessao()
    {
        var client = api.CreateClient();
        (await client.PostAsJsonAsync("/v1/exemption-batches", new { sessionId = Guid.NewGuid(), priorities = new[] { 7 } })).StatusCode
            .Should().Be(HttpStatusCode.BadRequest);
        var unknown = await client.PostAsJsonAsync("/v1/exemption-batches", new { sessionId = Guid.NewGuid(), priorities = new[] { 1 } });
        unknown.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await unknown.ProblemCodeAsync()).Should().Be("Exemption.SessionNotFound");
    }

    [Fact]
    public async Task Listagem_de_lotes_responde_200() =>
        (await api.CreateClient().GetAsync("/v1/exemption-batches")).StatusCode.Should().Be(HttpStatusCode.OK);
}
