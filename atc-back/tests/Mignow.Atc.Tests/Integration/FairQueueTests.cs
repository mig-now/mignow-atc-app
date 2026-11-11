using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Ports.Execution;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Infra.Database.Context;

namespace Mignow.Atc.Tests.Integration;

/// <summary>
/// Fila justa por cliente (Postgres real): com o cliente A já rodando, a próxima vaga vai para o cliente B, mesmo que a
/// execução de A tenha entrado antes na fila. Clientes diferentes rodam ao mesmo tempo.
/// </summary>
[Collection(IntegrationCollection.Name)]
[Trait("Category", "Integration")]
public sealed class FairQueueTests(AtcApiFactory api)
{
    [Fact]
    public async Task Vaga_vai_para_o_cliente_sem_execucao_rodando()
    {
        var client = api.CreateClient();
        await api.ProcessQueuedAsync(); // fila vazia antes de começar
        var (customerA, a1) = await TestData.CustomerWithSystemAsync(client, api.NewSimulatedSap().Host);
        var a2 = await client.PostAsJsonAsync("/v1/sap-systems", new
        {
            customerId = customerA, sid = "QAS", client = "200", host = api.NewSimulatedSap().Host, adtPort = 0, https = false, language = "EN",
            isProduction = false, projectType = "Conversion", author = "AUT-TS", username = "MIGATC",
        }).DataAsync<SapSystemDto>();
        var (_, b1) = await TestData.CustomerWithSystemAsync(client, api.NewSimulatedSap().Host);

        var runs = new Dictionary<Guid, Guid>();
        foreach (var system in new[] { a1, a2, b1 }) // nesta ordem: A1, A2 e só depois B1
        {
            var accepted = await client.PostAsJsonAsync("/v1/extractions", new { sapSystemId = system.Id, force = false });
            accepted.StatusCode.Should().Be(HttpStatusCode.Accepted);
            runs[system.Id] = (await accepted.DataAsync<IdDto>()).Id;
        }

        var first = await ClaimAsync();
        var second = await ClaimAsync();
        (await RunGuidAsync(first)).Should().Be(runs[a1.Id], "a fila vazia de execuções rodando segue a ordem de chegada");
        (await RunGuidAsync(second)).Should().Be(runs[b1.Id], "o cliente A já tem uma rodando: a vaga vai para o B");

        var processor = api.Services.GetRequiredService<RunProcessor>();
        await processor.ProcessAsync(first, CancellationToken.None);
        await processor.ProcessAsync(second, CancellationToken.None);
        await api.ProcessQueuedAsync();
        foreach (var id in runs.Values)
            (await client.GetAsync($"/v1/runs/{id}").DataAsync<JsonElement>()).GetProperty("status").GetString().Should().Be("Completed");
    }

    private async Task<int> ClaimAsync()
    {
        await using var scope = api.Services.CreateAsyncScope();
        var id = await scope.ServiceProvider.GetRequiredService<IRunQueue>().ClaimNextAsync(CancellationToken.None);
        id.Should().NotBeNull();
        return id!.Value;
    }

    /// <summary>A fila devolve o id interno; a API expõe o Guid.</summary>
    private async Task<Guid> RunGuidAsync(int internalId)
    {
        await using var scope = api.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AtcDbContext>();
        return await db.Set<Run>().Where(r => r.Id == internalId).Select(r => r.Guid).SingleAsync();
    }
}
