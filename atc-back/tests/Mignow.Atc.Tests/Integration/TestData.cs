using System.Net.Http.Json;

namespace Mignow.Atc.Tests.Integration;

/// <summary>Cadastro mínimo para os testes: cliente + ambiente (simulador por padrão).</summary>
public static class TestData
{
    public static async Task<(Guid CustomerId, SapSystemDto System)> CustomerWithSystemAsync(HttpClient client, string host, bool isProduction = false)
    {
        var suffix = Guid.NewGuid().ToString("N")[..6];
        var customer = await client.PostAsJsonAsync("/v1/customers", new { name = $"Cliente {suffix}", code = suffix }).DataAsync<IdDto>();
        var system = await client.PostAsJsonAsync("/v1/sap-systems", new
        {
            customerId = customer.Id, sid = "SBX", client = "100", host, adtPort = 0, https = false, language = "EN", isProduction,
            projectType = "Conversion", author = "AUT-TS", username = "MIGATC",
        }).DataAsync<SapSystemDto>();
        return (customer.Id, system);
    }
}
