using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Mignow.Atc.Tests.Integration;

namespace Mignow.Atc.Tests.Extraction;

/// <summary>
/// Extração ponta a ponta no simulador (Postgres local). Depende das colunas novas de <c>TB_ATC_EXTRACTION</c>
/// (<c>ColumnsJson</c>, <c>ExecutionJson</c>, <c>IsComplete</c>): só passa depois da migration coordenada.
/// </summary>
[Collection(IntegrationCollection.Name)]
[Trait("Category", "Integration")]
public sealed class ExtractionApiTests(AtcApiFactory api)
{
    [Fact]
    public async Task Verifica_extrai_nao_reextrai_e_exporta_os_tres_csvs()
    {
        var client = api.CreateClient();
        var (host, _) = api.NewSimulatedSap();
        var (_, system) = await TestData.CustomerWithSystemAsync(client, host);

        var latest = await client.GetAsync($"/v1/sap-systems/{system.Id}/atc/latest").DataAsync<System.Text.Json.JsonElement>();
        latest.GetProperty("alreadyExtracted").GetBoolean().Should().BeFalse();
        latest.GetProperty("series").GetString().Should().StartWith("ZMIG_");

        var run = await client.PostAsJsonAsync("/v1/extractions", new { sapSystemId = system.Id, force = false }).DataAsync<RunDto>();
        await api.ProcessQueuedAsync();
        var done = await client.GetAsync($"/v1/runs/{run.Id}").DataAsync<RunDto>();
        done.Status.Should().Be("Completed");
        done.Result!.Value.GetProperty("situation").GetString().Should().Be("Extracted");
        var extractionId = done.Result!.Value.GetProperty("extractionId").GetGuid();

        var extraction = await client.GetAsync($"/v1/extractions/{extractionId}").DataAsync<System.Text.Json.JsonElement>();
        extraction.GetProperty("totalCount").GetInt32().Should().BeGreaterThan(0);
        var findings = await client.GetAsync($"/v1/extractions/{extractionId}/findings?includeColumns=true&pageSize=5").DataAsync<System.Text.Json.JsonElement>();
        findings.GetProperty("items").GetArrayLength().Should().BeGreaterThan(0);

        foreach (var kind in new[] { "total", "novos", "remediacao" })
        {
            var file = await client.GetAsync($"/v1/extractions/{extractionId}/files/{kind}?priorities=1,2,3");
            file.StatusCode.Should().Be(HttpStatusCode.OK);
            (await file.Content.ReadAsByteArrayAsync()).Take(3).Should().Equal(Encoding.UTF8.GetPreamble());
        }

        // Mesma execução: nada a extrair (a não ser com force).
        var again = await client.PostAsJsonAsync("/v1/extractions", new { sapSystemId = system.Id, force = false }).DataAsync<RunDto>();
        await api.ProcessQueuedAsync();
        (await client.GetAsync($"/v1/runs/{again.Id}").DataAsync<RunDto>()).Result!.Value.GetProperty("situation").GetString().Should().Be("AlreadyExtracted");
        (await client.GetAsync($"/v1/extractions?sapSystemId={system.Id}").DataAsync<PagedDto<System.Text.Json.JsonElement>>()).Items.Should().ContainSingle();
    }
}
