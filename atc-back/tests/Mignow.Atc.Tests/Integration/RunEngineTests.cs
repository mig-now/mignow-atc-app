using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Mignow.Atc.Tests.Integration;

/// <summary>Motor ponta a ponta com o pipeline ConnectionTest: fila, lock por ambiente (409 com o run), cancelamento e retry.</summary>
[Collection(IntegrationCollection.Name)]
[Trait("Category", "Integration")]
public sealed class RunEngineTests(AtcApiFactory api)
{
    [Fact]
    public async Task Run_de_teste_de_conexao_conclui_e_grava_no_ambiente()
    {
        var client = api.CreateClient();
        var (host, _) = api.NewSimulatedSap();
        var (_, system) = await TestData.CustomerWithSystemAsync(client, host);

        var response = await client.PostAsync($"/v1/sap-systems/{system.Id}/test/run", null);
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var queued = await response.DataAsync<RunDto>();
        queued.Status.Should().Be("Queued");
        queued.Kind.Should().Be("ConnectionTest");
        queued.SubjectId.Should().Be(system.Id);
        queued.Steps.Should().ContainSingle(s => s.Code == "TEST_CONNECTION");

        await api.ProcessQueuedAsync();

        var run = await client.GetAsync($"/v1/runs/{queued.Id}").DataAsync<RunDto>();
        run.Status.Should().Be("Completed");
        run.Steps.Single().Status.Should().Be("Completed");
        run.Steps.Single().ProgressDone.Should().Be(1);
        run.Result!.Value.GetProperty("ok").GetBoolean().Should().BeTrue();
        run.Result!.Value.GetProperty("clientCategory").GetString().Should().Be("T");
        var events = await client.GetAsync($"/v1/runs/{run.Id}/events").DataAsync<PagedDto<EventDto>>();
        events.Items.Select(e => e.Code).Should().Contain(["Run.Started", "Sap.ConnectionOk", "Run.Completed"]);
        (await client.GetAsync($"/v1/runs/{run.Id}/ai-calls")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.GetAsync($"/v1/sap-systems/{system.Id}").DataAsync<SapSystemDto>()).LastConnectionTestOk.Should().BeTrue();
    }

    [Fact]
    public async Task Uma_execucao_ativa_por_ambiente_responde_409_com_o_run_que_bloqueia()
    {
        var client = api.CreateClient();
        var (host, _) = api.NewSimulatedSap();
        var (_, system) = await TestData.CustomerWithSystemAsync(client, host);
        var first = await client.PostAsync($"/v1/sap-systems/{system.Id}/test/run", null).DataAsync<RunDto>();

        var second = await client.PostAsync($"/v1/sap-systems/{system.Id}/test/run", null);
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await second.ProblemAsync();
        problem.GetProperty("detail").GetString().Should().Be("Run.EnvironmentBusy");
        problem.GetProperty("data").GetProperty("id").GetGuid().Should().Be(first.Id);

        var active = await client.GetAsync($"/v1/runs?sapSystemId={system.Id}&activeOnly=true").DataAsync<PagedDto<RunDto>>();
        active.Items.Should().ContainSingle().Which.Id.Should().Be(first.Id);

        // Cancelar na fila libera o ambiente na hora.
        var cancelled = await client.PostAsync($"/v1/runs/{first.Id}/cancel", null).DataAsync<RunDto>();
        cancelled.Status.Should().Be("Cancelled");
        (await client.PostAsync($"/v1/sap-systems/{system.Id}/test/run", null)).StatusCode.Should().Be(HttpStatusCode.Accepted);
        await api.ProcessQueuedAsync();
    }

    [Fact]
    public async Task Falha_no_passo_e_retry_volta_para_a_fila_no_passo_que_falhou()
    {
        var client = api.CreateClient();
        var (_, system) = await TestData.CustomerWithSystemAsync(client, "sim://cenario-que-nao-existe");
        var run = await client.PostAsync($"/v1/sap-systems/{system.Id}/test/run", null).DataAsync<RunDto>();
        await api.ProcessQueuedAsync();

        var failed = await client.GetAsync($"/v1/runs/{run.Id}").DataAsync<RunDto>();
        failed.Status.Should().Be("Failed");
        failed.FailureCode.Should().Be("Sap.ScenarioNotFound");
        failed.Steps.Single().Status.Should().Be("Error");

        var retry = await client.PostAsync($"/v1/runs/{run.Id}/retry", null);
        retry.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var queued = await retry.DataAsync<RunDto>();
        queued.Status.Should().Be("Queued");
        queued.Steps.Single().Status.Should().Be("Pending");
        await api.ProcessQueuedAsync();

        (await client.PostAsync($"/v1/runs/{run.Id}/cancel", null)).StatusCode.Should().Be(HttpStatusCode.Conflict, "já terminou");
    }

    [Fact]
    public async Task Run_inexistente_da_404() =>
        (await api.CreateClient().GetAsync($"/v1/runs/{Guid.NewGuid()}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
}
