using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace Mignow.Atc.Tests.Integration;

/// <summary>Cadastro das regras de triagem pela API: lista por tipo de projeto, cria regra nova, edita a padrão, exclui só a nova.</summary>
[Collection(IntegrationCollection.Name)]
[Trait("Category", "Integration")]
public sealed class TriageRulesApiTests(AtcApiFactory api)
{
    private static object Body(string description, string scope, bool active, string[]? refs = null, string? code = null, string effect = "Isenção",
        string? objectPattern = null, Dictionary<string, string[]>? parameters = null) =>
        new { code, description, effect, scope, active, refObjects = refs, objectPattern, messagePattern = (string?)null, obs = (string?)null, parameters };

    [Fact]
    public async Task Cria_edita_e_exclui_regras()
    {
        var client = api.CreateClient();
        var all = await client.GetAsync("/v1/triage-rules").DataAsync<List<JsonElement>>();
        all.Single(r => r.GetProperty("code").GetString() == "R9").GetProperty("scope").GetString().Should().Be("Upgrade");
        all.Single(r => r.GetProperty("code").GetString() == "R1").GetProperty("scope").GetString().Should().Be("All");

        // nova: sem critério recusa; com critério entra no fim, só para conversão
        (await client.PostAsJsonAsync("/v1/triage-rules", Body("sem critério", "All", true, code: "ZAPI1"))).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var created = await client.PostAsJsonAsync("/v1/triage-rules", Body("Legado YMIG", "Conversion", true, code: "zapi1", objectPattern: "YMIG*"))
            .DataAsync<JsonElement>();
        created.GetProperty("code").GetString().Should().Be("ZAPI1");
        created.GetProperty("builtIn").GetBoolean().Should().BeFalse();
        (await client.PostAsJsonAsync("/v1/triage-rules", Body("x", "All", true, code: "R1", objectPattern: "Y*"))).StatusCode.Should().Be(HttpStatusCode.Conflict);
        var upgrade = await client.GetAsync("/v1/triage-rules?projectType=Upgrade").DataAsync<List<JsonElement>>();
        upgrade.Select(r => r.GetProperty("code").GetString()).Should().NotContain("ZAPI1");

        // padrão: muda descrição/escopo/objetos; o efeito fica no código; sem objetos é recusado
        var original = all.Single(r => r.GetProperty("code").GetString() == "R9");
        try
        {
            (await client.PutAsJsonAsync("/v1/triage-rules/R9", Body("R9", "All", true, []))).StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var r9 = await client.PutAsJsonAsync("/v1/triage-rules/R9", Body("R9 em todos", "All", true, ["bseg", "mara"], effect: "Cancelado"))
                .DataAsync<JsonElement>();
            r9.GetProperty("effect").GetString().Should().Be("Isenção");
            r9.GetProperty("refObjects").EnumerateArray().Select(e => e.GetString()).Should().Equal("BSEG", "MARA");
            var conversion = await client.GetAsync("/v1/triage-rules?projectType=Conversion").DataAsync<List<JsonElement>>();
            conversion.Select(r => r.GetProperty("code").GetString()).Should().Contain(["R9", "ZAPI1"]);
        }
        finally
        {
            await client.PutAsJsonAsync("/v1/triage-rules/R9", Body(original.GetProperty("description").GetString()!, "Upgrade", true,
                [.. original.GetProperty("refObjects").EnumerateArray().Select(e => e.GetString()!)]));
        }

        // renomear: a nova vira ZAPI2 (chave continua ZAPI1); código repetido é recusado
        (await client.PutAsJsonAsync("/v1/triage-rules/ZAPI1", Body("Legado", "Conversion", true, code: "R2", objectPattern: "YMIG*")))
            .StatusCode.Should().Be(HttpStatusCode.Conflict);
        var renamed = await client.PutAsJsonAsync("/v1/triage-rules/ZAPI1", Body("Legado", "Conversion", true, code: "zapi2", objectPattern: "YMIG*"))
            .DataAsync<JsonElement>();
        renamed.GetProperty("code").GetString().Should().Be("ZAPI2");
        renamed.GetProperty("key").GetString().Should().Be("ZAPI1");

        // padrão: parâmetro da lógica (R8 só UPDATE), parâmetro desconhecido recusado; renomear e voltar
        var r8 = all.Single(r => r.GetProperty("code").GetString() == "R8");
        try
        {
            (await client.PutAsJsonAsync("/v1/triage-rules/R8", Body("R8", "All", true, parameters: new() { ["xx"] = ["A"] })))
                .StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var edited = await client.PutAsJsonAsync("/v1/triage-rules/R8",
                Body("R8", "All", true, code: "ZR8", parameters: new() { ["operations"] = ["update"], ["refTypes"] = ["TABL"] })).DataAsync<JsonElement>();
            edited.GetProperty("code").GetString().Should().Be("ZR8");
            edited.GetProperty("key").GetString().Should().Be("R8");
            edited.GetProperty("parameters").GetProperty("operations").EnumerateArray().Select(e => e.GetString()).Should().Equal("UPDATE");
            edited.GetProperty("condition").GetString().Should().Contain("seguido de UPDATE");
        }
        finally
        {
            await client.PutAsJsonAsync("/v1/triage-rules/ZR8", Body(r8.GetProperty("description").GetString()!, "All", true, code: "R8",
                parameters: new() { ["operations"] = ["MODIFY", "UPDATE", "INSERT"], ["refTypes"] = ["TABL", "VIEW"] }));
        }

        (await client.DeleteAsync("/v1/triage-rules/R1")).StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await client.DeleteAsync("/v1/triage-rules/ZAPI2")).IsSuccessStatusCode.Should().BeTrue();
        (await client.GetAsync("/v1/triage-rules").DataAsync<List<JsonElement>>()).Select(r => r.GetProperty("code").GetString()).Should().NotContain("ZAPI2");
    }
}
