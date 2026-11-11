using System.Collections.Concurrent;
using System.Text.Json;
using Anthropic;
using Anthropic.Models.Messages;
using Microsoft.Extensions.Logging;
using Mignow.Atc.Infra.External.Ai.Providers;

namespace Mignow.Atc.Infra.External.Ai.Anthropic;

/// <summary>
/// Claude pela API Anthropic (SDK oficial). Saída imposta por JSON Schema (structured outputs, sem prefill e sem
/// tool_choice forçado); texto de sistema com cache de prompt; esforço explícito. Não envia temperature. Sempre em
/// streaming: o teto pedido é limitado ao máximo do modelo, consultado na Models API e guardado em memória.
/// </summary>
public sealed class AnthropicJsonClient(IHttpClientFactory http, ILogger<AnthropicJsonClient> logger)
{
    /// <summary>Cliente HTTP nomeado sem timeout próprio: quem limita cada tentativa é o SDK (<see cref="AiLimits.AnthropicTimeout"/>).</summary>
    public const string ClientName = "anthropic";

    private readonly ConcurrentDictionary<string, AnthropicClient> _clients = new();
    private readonly ConcurrentDictionary<string, int> _modelMax = new(StringComparer.OrdinalIgnoreCase);

    public async Task<AiCompletion> CompleteAsync(ResolvedAiProvider provider, string system, string user, JsonElement schema, Effort effort,
        int maxTokens, CancellationToken ct)
    {
        var client = ClientFor(provider);
        var limit = Math.Min(maxTokens, await ModelMaxAsync(client, provider.Model, ct));
        var message = await client.Messages.CreateStreaming(new MessageCreateParams
        {
            Model = provider.Model,
            MaxTokens = limit,
            System = new List<TextBlockParam> { new() { Text = system, CacheControl = new CacheControlEphemeral() } },
            Messages = [new() { Role = Role.User, Content = user }],
            OutputConfig = new OutputConfig
            {
                Effort = effort,
                Format = new JsonOutputFormat { Schema = schema.EnumerateObject().ToDictionary(p => p.Name, p => p.Value.Clone()) },
            },
        }, ct).Aggregate();
        var text = string.Concat(message.Content.Select(b => b.Value).OfType<TextBlock>().Select(t => t.Text));
        var stop = message.StopReason?.Raw(); // valor da API ("max_tokens", "refusal"...), não o nome do enum do SDK
        logger.LogInformation("IA: {Model} respondeu ({Stop}, teto {Limit}, esforço {Effort}) com {In} tokens de entrada ({Cached} do cache) e {Out} de saída",
            provider.Model, stop, limit, effort, message.Usage.InputTokens, message.Usage.CacheReadInputTokens, message.Usage.OutputTokens);
        return new AiCompletion(text, message.Usage.InputTokens + (message.Usage.CacheReadInputTokens ?? 0) + (message.Usage.CacheCreationInputTokens ?? 0),
            message.Usage.OutputTokens, stop);
    }

    /// <summary>Máximo de max_tokens do modelo (Models API). Falhou a consulta: teto seguro, sem guardar (tenta de novo depois).</summary>
    private async Task<int> ModelMaxAsync(AnthropicClient client, string model, CancellationToken ct)
    {
        if (_modelMax.TryGetValue(model, out var known))
            return known;
        try
        {
            var info = await client.WithOptions(o => o with { Timeout = TimeSpan.FromSeconds(15), MaxRetries = 1 })
                .Models.Retrieve(model, cancellationToken: ct);
            if (info.MaxTokens is not { } reported)
                return AiLimits.FallbackModelMax;
            var max = (int)Math.Clamp(reported, 1_024, 1_000_000);
            _modelMax[model] = max;
            return max;
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            logger.LogWarning(ex, "IA: não foi possível consultar o máximo de saída de {Model}; usando {Fallback}", model, AiLimits.FallbackModelMax);
            return AiLimits.FallbackModelMax;
        }
    }

    private AnthropicClient ClientFor(ResolvedAiProvider provider) =>
        _clients.GetOrAdd(provider.ApiKey ?? string.Empty, _ => new AnthropicClient
        {
            ApiKey = provider.ApiKey,
            Timeout = AiLimits.AnthropicTimeout,
            HttpClient = http.CreateClient(ClientName),
        });
}
