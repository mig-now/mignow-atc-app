using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace Mignow.Atc.Tests.Integration;

/// <summary>Monitor de extrações: dois ambientes do mesmo cliente disparados juntos e acompanhados pela mesma consulta.</summary>
[Collection(IntegrationCollection.Name)]
[Trait("Category", "Integration")]
public sealed class ExtractionMonitorTests(AtcApiFactory api)
{
    [Fact]
    public async Task Monitor_lista_os_ambientes_do_cliente_com_a_ultima_execucao_e_a_ultima_extracao()
    {
        var client = api.CreateClient();
        var (customerId, first) = await TestData.CustomerWithSystemAsync(client, api.NewSimulatedSap().Host);
        var second = await client.PostAsJsonAsync("/v1/sap-systems", new
        {
            customerId, sid = "QAS", client = "200", host = api.NewSimulatedSap().Host, adtPort = 0, https = false, language = "EN",
            isProduction = false, projectType = "Conversion", author = "AUT-TS", username = "MIGATC",
        }).DataAsync<SapSystemDto>();

        var before = await client.GetAsync($"/v1/extractions/monitor?customerId={customerId}").DataAsync<List<JsonElement>>();
        before.Select(r => r.GetProperty("sapSystemName").GetString()).Should().Equal("QAS/200", "SBX/100");
        before.Should().OnlyContain(r => r.GetProperty("lastRun").ValueKind == JsonValueKind.Null
            && r.GetProperty("lastExtraction").ValueKind == JsonValueKind.Null);

        foreach (var id in new[] { first.Id, second.Id })
            (await client.PostAsJsonAsync("/v1/extractions", new { sapSystemId = id, force = false })).StatusCode.Should().Be(HttpStatusCode.Accepted);
        var queued = await client.GetAsync($"/v1/extractions/monitor?customerId={customerId}").DataAsync<List<JsonElement>>();
        queued.Should().OnlyContain(r => r.GetProperty("lastRun").GetProperty("status").GetString() == "Queued");

        await api.ProcessQueuedAsync();
        var after = await client.GetAsync($"/v1/extractions/monitor?customerId={customerId}").DataAsync<List<JsonElement>>();
        after.Should().HaveCount(2).And.OnlyContain(r => r.GetProperty("lastRun").GetProperty("status").GetString() == "Completed"
            && r.GetProperty("lastRun").GetProperty("kind").GetString() == "Extraction"
            && r.GetProperty("lastExtraction").GetProperty("totalCount").GetInt32() > 0);
    }
}
