using System.Text.Json;
using Anthropic.Models.Messages;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Ai;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Infra.External.Ai.Anthropic;
using Mignow.Atc.Infra.External.Ai.Fake;
using Mignow.Atc.Infra.External.Ai.Providers;

namespace Mignow.Atc.Infra.External.Ai;

/// <summary>
/// IA: usa o provedor padrão do cadastro (o mesmo modelo para todos os usos; o uso define esforço e teto), chama com o JSON Schema do pedido e desserializa a resposta
/// (inválida, recusada ou truncada = falha). Toda chamada que chega ao modelo é informada em <c>onCall</c>, inclusive as
/// que falham. Cortada pelo teto: uma nova chamada com esforço menor. O Fake só responde em Development/Local/Testing.
/// </summary>
public sealed class AiGateway(AiProviderResolver providers, AnthropicJsonClient anthropic, IEnumerable<IFakeAiResponder> fakes, IHostEnvironment env,
    IStringLocalizer<Errors> l, ILogger<AiGateway> logger) : IAiGateway
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Result<T>> CompleteAsync<T>(AiRequest request, Action<AiUsageInfo> onCall, CancellationToken ct)
    {
        var provider = await providers.DefaultAsync(ct);
        if (provider is null)
            return AiErrors.NotConfigured(l, request.Usage.ToString());
        if (provider.Kind == AiProviderKind.Fake)
            return Fake<T>(provider, request, onCall);
        if (string.IsNullOrWhiteSpace(provider.ApiKey))
            return AiErrors.NotConfigured(l, $"{provider.Model}: chave de API");

        var effort = request.Effort;
        var completion = await CallAsync(provider, request, effort, onCall, ct);
        if (completion is { IsSuccess: true, Value.StopReason: "max_tokens" } && Lower(effort) is { } lower)
        {
            logger.LogWarning("IA: resposta de {Model} ({Purpose}) cortada pelo teto com esforço {Effort}; nova chamada com {Lower}", provider.Model,
                request.Purpose, effort, lower);
            completion = await CallAsync(provider, request, lower, onCall, ct);
        }
        if (completion.IsFailure)
            return completion.Error;
        if (completion.Value.StopReason is "refusal" or "max_tokens" or "pause_turn")
            return AiErrors.Failed(l, $"stop_reason {completion.Value.StopReason}");
        return Parse<T>(completion.Value.Json);
    }

    private async Task<Result<AiCompletion>> CallAsync(ResolvedAiProvider provider, AiRequest request, AiEffort effort, Action<AiUsageInfo> onCall,
        CancellationToken ct)
    {
        try
        {
            var completion = await anthropic.CompleteAsync(provider, request.System, request.User, request.Schema, ToSdk(effort),
                request.MaxOutputTokens ?? AiLimits.Default(request.Usage), ct);
            var ok = completion.StopReason is not ("refusal" or "max_tokens" or "pause_turn");
            onCall(new AiUsageInfo(provider.Model, (int)completion.InputTokens, (int)completion.OutputTokens, completion.Cost, ok,
                ok ? null : $"stop_reason:{completion.StopReason}"));
            return completion;
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            logger.LogWarning(ex, "IA: chamada ao modelo {Model} falhou ({Purpose})", provider.Model, request.Purpose);
            return AiErrors.Failed(l, ex.GetType().Name);
        }
    }

    private Result<T> Fake<T>(ResolvedAiProvider provider, AiRequest request, Action<AiUsageInfo> onCall)
    {
        if (!(env.IsDevelopment() || env.IsEnvironment("Local") || env.IsEnvironment("Testing")))
            return AiErrors.NotConfigured(l, "fake");
        var responder = fakes.FirstOrDefault(f => f.Purposes.Contains(request.Purpose, StringComparer.Ordinal));
        if (responder is null)
            return AiErrors.NotConfigured(l, $"fake:{request.Purpose}");
        var json = responder.Respond(request);
        // Tokens estimados (~4 caracteres por token), custo zero: o consumo aparece também em desenvolvimento.
        onCall(new AiUsageInfo(provider.Model, Math.Max(1, (request.System.Length + request.User.Length) / 4), Math.Max(1, json.Length / 4), 0m));
        return Parse<T>(json);
    }

    private Result<T> Parse<T>(string json)
    {
        try
        {
            var value = JsonSerializer.Deserialize<T>(json, JsonOptions);
            return value is null ? AiErrors.Invalid(l, "null") : value;
        }
        catch (JsonException ex)
        {
            return AiErrors.Invalid(l, ex.Path ?? "json");
        }
    }

    private static Effort ToSdk(AiEffort effort) => effort switch
    {
        AiEffort.Low => Effort.Low,
        AiEffort.Medium => Effort.Medium,
        _ => Effort.High,
    };

    private static AiEffort? Lower(AiEffort effort) => effort switch
    {
        AiEffort.High => AiEffort.Medium,
        AiEffort.Medium => AiEffort.Low,
        _ => null,
    };
}
