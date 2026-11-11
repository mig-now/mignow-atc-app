using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Ports.Execution;
using Mignow.Atc.Infra.External.Sap.Simulator;

namespace Mignow.Atc.Tests.Integration;

/// <summary>
/// API no ambiente Testing (banco MIGNOW_ATC_TEST, seed de demonstração, IA Fake) SEM o worker em segundo plano: o teste
/// processa a fila quando quer (<see cref="ProcessQueuedAsync"/>), com o mesmo RunProcessor do worker.
/// </summary>
public sealed class AtcApiFactory : WebApplicationFactory<Program>
{
    public AtcApiFactory() => TestDatabase.ResetOnce();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Default", TestDatabase.ConnectionString);
        builder.UseSetting("Security:SecretKey", TestDatabase.SecretKey);
        builder.UseSetting("Database:MigrateOnStartup", "true");
        builder.UseSetting("Database:SeedDemoData", "true");
        builder.UseSetting("Engine:Enabled", "false");
        builder.UseSetting("Engine:ProgressSaveMilliseconds", "0");
    }

    /// <summary>Processa o que estiver na fila, como o worker faria.</summary>
    public async Task ProcessQueuedAsync()
    {
        var processor = Services.GetRequiredService<RunProcessor>();
        while (true)
        {
            int? id;
            await using (var scope = Services.CreateAsyncScope())
                id = await scope.ServiceProvider.GetRequiredService<IRunQueue>().ClaimNextAsync(CancellationToken.None);
            if (id is null)
                return;
            await processor.ProcessAsync(id.Value, CancellationToken.None);
        }
    }

    /// <summary>Um "SAP" novo (cenário migracao-tipica) só deste teste: <c>sim://nome</c>.</summary>
    public (string Host, SimulatedSapState State) NewSimulatedSap(Func<SapScenario, SapScenario>? tweak = null)
    {
        var registry = Services.GetRequiredService<SimulatedSapRegistry>();
        var scenario = registry.LoadScenario("migracao-tipica") ?? throw new InvalidOperationException("cenário migracao-tipica não encontrado");
        var name = $"t{Guid.NewGuid():N}"[..12];
        var state = registry.Register(name, tweak is null ? scenario : tweak(scenario));
        return ($"sim://{name}", state);
    }
}

[CollectionDefinition(Name)]
public sealed class IntegrationCollection : ICollectionFixture<AtcApiFactory>
{
    public const string Name = "Integration";
}

/// <summary>Envelope e chamadas comuns.</summary>
public static class Api
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    public sealed record Envelope<T>(bool Success, T? Data, string? Message);

    public static async Task<T> DataAsync<T>(this HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"HTTP {(int)response.StatusCode}: {body}");
        return JsonSerializer.Deserialize<Envelope<T>>(body, Json)!.Data!;
    }

    public static async Task<T> DataAsync<T>(this Task<HttpResponseMessage> response) => await (await response).DataAsync<T>();

    public static async Task<JsonElement> ProblemAsync(this HttpResponseMessage response) =>
        JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());

    public static async Task<string?> ProblemCodeAsync(this HttpResponseMessage response) =>
        (await response.ProblemAsync()).TryGetProperty("detail", out var d) ? d.GetString() : null;
}
