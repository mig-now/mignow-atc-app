using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Mignow.Atc.Tests.Integration;

[Collection(IntegrationCollection.Name)]
[Trait("Category", "Integration")]
public sealed class RegistryApiTests(AtcApiFactory api)
{
    [Fact]
    public async Task Usuario_de_desenvolvimento_e_consultor()
    {
        var me = await api.CreateClient().GetAsync("/v1/me").DataAsync<MeDto>();
        me.Email.Should().Be("dev@migatc.local");
        me.Role.Should().Be("Consultant");
    }

    [Fact]
    public async Task Health_responde_ok() =>
        (await api.CreateClient().GetAsync("/v1/health")).StatusCode.Should().Be(HttpStatusCode.OK);

    [Fact]
    public async Task Seed_tem_cliente_demo_ambiente_simulado_e_ia_fake_padrao()
    {
        var client = api.CreateClient();
        var systems = await client.GetAsync("/v1/sap-systems?search=SBX&pageSize=200").DataAsync<PagedDto<SapSystemDto>>();
        systems.Items.Should().Contain(s => s.Host == "sim://migracao-tipica" && s.IsSimulator);
        var providers = await client.GetAsync("/v1/ai-providers").DataAsync<List<AiProviderDto>>();
        // Um só modelo para todos os usos: Claude Opus cadastrado; o padrão da demonstração é o Fake (único padrão).
        providers.Where(p => p.Kind == "Anthropic").Select(p => p.Model).Should().BeEquivalentTo("claude-opus-5-5");
        providers.Where(p => p.IsDefault).Should().ContainSingle().Which.Kind.Should().Be("Fake");
    }

    [Fact]
    public async Task Ambiente_real_com_senha_que_nunca_volta()
    {
        var client = api.CreateClient();
        var customer = await client.PostAsJsonAsync("/v1/customers", new { name = $"Cliente {Guid.NewGuid():N}" }).DataAsync<IdDto>();
        const string password = "S3nh@-Secreta-Que-Nao-Volta";
        var created = await client.PostAsJsonAsync("/v1/sap-systems", new
        {
            customerId = customer.Id, sid = "DI4", client = "400", host = "10.99.80.8", adtPort = 44332, https = true, language = "EN",
            isProduction = false, projectType = "Conversion", author = "AUT-AB", username = "migatc", password,
        });
        var raw = await created.Content.ReadAsStringAsync();
        created.StatusCode.Should().Be(HttpStatusCode.OK, raw);
        raw.Should().NotContain(password);
        var system = await created.DataAsync<SapSystemDto>();
        system.HasPassword.Should().BeTrue();
        (await client.GetStringAsync($"/v1/sap-systems?customerId={customer.Id}")).Should().NotContain(password);
        (await client.PutAsJsonAsync($"/v1/sap-systems/{system.Id}/password", new { password = "outra" })).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.DeleteAsync($"/v1/customers/{customer.Id}")).StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Ambiente_sem_senha_fora_do_simulador_e_recusado()
    {
        var client = api.CreateClient();
        var customer = await client.PostAsJsonAsync("/v1/customers", new { name = $"Cliente {Guid.NewGuid():N}" }).DataAsync<IdDto>();
        var response = await client.PostAsJsonAsync("/v1/sap-systems", new
        {
            customerId = customer.Id, sid = "DI4", client = "400", host = "10.99.80.8", adtPort = 44332, https = true, language = "EN",
            projectType = "Upgrade", author = "AUT-AB", username = "u",
        });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Cliente_duplicado_da_409_com_codigo()
    {
        var client = api.CreateClient();
        var name = $"Duplicado {Guid.NewGuid():N}";
        (await client.PostAsJsonAsync("/v1/customers", new { name })).EnsureSuccessStatusCode();
        var again = await client.PostAsJsonAsync("/v1/customers", new { name = name.ToUpperInvariant() });
        again.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await again.ProblemCodeAsync()).Should().Be("Customer.NameAlreadyExists");
    }

    [Fact]
    public async Task Validacao_volta_erros_por_campo_no_idioma_pedido()
    {
        var client = api.CreateClient();
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("pt-BR");
        var response = await client.PostAsJsonAsync("/v1/customers", new { name = "" });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"errors\"").And.Contain("name").And.Contain("Alguns campos estão inválidos.");
    }

    [Fact]
    public async Task Parametros_tem_os_padroes_das_skills_e_validam_limites()
    {
        var client = api.CreateClient();
        var s = await client.GetAsync("/v1/settings").DataAsync<SettingsDto>();
        s.KbPath.Should().Be("../.claude/skills/atc-remediacao/KB_ATC");
        s.MignowRulesPath.Should().Be("../.claude/skills/atc-extracao-mignow/regras/regras.json");
        s.BatchSize.Should().BeGreaterThan(0);
        (await client.PutAsJsonAsync("/v1/settings", s with { Parallelism = 17 })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await client.PutAsJsonAsync("/v1/settings", s with { MignowParallel = 10 })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var saved = await client.PutAsJsonAsync("/v1/settings", s).DataAsync<SettingsDto>();
        saved.Should().Be(s);
    }

    [Fact]
    public async Task Provedor_de_ia_crud_sem_devolver_a_chave()
    {
        var client = api.CreateClient();
        var created = await client.PostAsJsonAsync("/v1/ai-providers", new
        {
            name = "Teste", kind = "Anthropic", model = "claude-opus-5-5", apiKey = "sk-ant-segredo", isDefault = false,
        });
        var raw = await created.Content.ReadAsStringAsync();
        raw.Should().NotContain("sk-ant-segredo");
        var p = await created.DataAsync<AiProviderDto>();
        p.HasApiKey.Should().BeTrue();
        (await client.DeleteAsync($"/v1/ai-providers/{p.Id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Teste_de_conexao_sincrono_no_simulador_le_release_e_t000()
    {
        var client = api.CreateClient();
        var (host, _) = api.NewSimulatedSap();
        var (_, system) = await TestData.CustomerWithSystemAsync(client, host);
        var test = await client.PostAsync($"/v1/sap-systems/{system.Id}/test", null).DataAsync<ConnectionTestDto>();
        test.Ok.Should().BeTrue();
        test.ClientCategory.Should().Be("T");
        var after = await client.GetAsync($"/v1/sap-systems/{system.Id}").DataAsync<SapSystemDto>();
        after.LastConnectionTestOk.Should().BeTrue();
        after.ClientCategory.Should().Be("T");
    }

    [Fact]
    public async Task Swagger_tem_uma_tag_por_controller()
    {
        var json = await api.CreateClient().GetStringAsync("/swagger/v1/swagger.json");
        foreach (var tag in new[] { "Customers", "SapSystems", "Settings", "AiProviders", "Runs", "Me", "Health" })
            json.Should().Contain($"\"{tag}\"");
        json.Should().Contain("\"Runs_List\"");
    }
}
