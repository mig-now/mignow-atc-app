using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Mignow.Atc.Infra.External.Sap.Simulator;

/// <summary>Um estado por cenário (<c>sim://nome</c>), carregado do JSON na primeira chamada e mantido na memória do processo.</summary>
public sealed class SimulatedSapRegistry(IOptions<SapSimulatorOptions> options, IHostEnvironment env)
{
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };
    private readonly ConcurrentDictionary<string, SimulatedSapState> _states = new(StringComparer.OrdinalIgnoreCase);

    public SimulatedSapState? Get(string baseUrl)
    {
        var name = baseUrl["sim://".Length..].Trim('/').ToLowerInvariant();
        if (_states.TryGetValue(name, out var state))
            return state;
        var file = FindScenario(name);
        if (file is null)
            return null;
        var scenario = JsonSerializer.Deserialize<SapScenario>(File.ReadAllText(file), JsonOptions)
                       ?? throw new InvalidOperationException($"Cenário inválido: {file}");
        return _states.GetOrAdd(name, _ => new SimulatedSapState(scenario));
    }

    /// <summary>Testes: estado novo a partir de um cenário em memória.</summary>
    public SimulatedSapState Register(string name, SapScenario scenario) => _states[name.ToLowerInvariant()] = new SimulatedSapState(scenario);

    /// <summary>Lê um cenário da pasta configurada (testes partem dele).</summary>
    public SapScenario? LoadScenario(string name) =>
        FindScenario(name) is { } file ? JsonSerializer.Deserialize<SapScenario>(File.ReadAllText(file), JsonOptions) : null;

    public void Reset(string name) => _states.TryRemove(name, out _);

    private string? FindScenario(string name)
    {
        var configured = options.Value.ScenariosPath;
        var roots = Path.IsPathRooted(configured)
            ? [configured]
            : new[] { AppContext.BaseDirectory, env.ContentRootPath, Directory.GetCurrentDirectory() }
                .SelectMany(Ancestors).Select(d => Path.Combine(d, configured)).Distinct();
        return roots.Select(r => Path.Combine(r, $"{name}.json")).FirstOrDefault(File.Exists);
    }

    private static IEnumerable<string> Ancestors(string start)
    {
        for (var d = new DirectoryInfo(start); d is not null; d = d.Parent)
            yield return d.FullName;
    }
}
