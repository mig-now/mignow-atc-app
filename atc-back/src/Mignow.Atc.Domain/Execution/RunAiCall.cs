using Mignow.Atc.Domain.Common;
using Mignow.Atc.Domain.Registry;

namespace Mignow.Atc.Domain.Execution;

/// <summary>Uma chamada de IA da execução: uso, modelo, tokens, custo informado (US$, nulo = não informado) e se deu certo.</summary>
public sealed class RunAiCall : EntityBase
{
    private RunAiCall() { }

    public int RunId { get; private set; }

    public AiUsage Usage { get; private set; }

    public string Model { get; private set; } = string.Empty;

    public int InputTokens { get; private set; }

    public int OutputTokens { get; private set; }

    public decimal? Cost { get; private set; }

    public bool Ok { get; private set; }

    public string? ErrorCode { get; private set; }

    public string? ItemRef { get; private set; }

    public DateTimeOffset At { get; private set; }

    public static RunAiCall Create(int runId, AiUsage usage, string model, int inputTokens, int outputTokens, decimal? cost, bool ok,
        string? errorCode, string? itemRef, DateTimeOffset at) =>
        new()
        {
            RunId = runId, Usage = usage, Model = Guard.Required(model, "RunAiCall.ModelRequired", 120), InputTokens = Math.Max(0, inputTokens),
            OutputTokens = Math.Max(0, outputTokens), Cost = cost, Ok = ok, ErrorCode = Guard.Optional(errorCode, 100),
            ItemRef = Guard.Optional(itemRef, 200), At = at,
        };
}
