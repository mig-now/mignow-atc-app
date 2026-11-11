using Mignow.Atc.Domain.Common;

namespace Mignow.Atc.Domain.Execution;

/// <summary>Log da execução. A mensagem é um código i18n + argumentos (o front traduz). <see cref="ItemRef"/> = item/objeto.</summary>
public sealed class RunEvent : EntityBase
{
    private RunEvent() { }

    public int RunId { get; private set; }

    public DateTimeOffset At { get; private set; }

    public EventLevel Level { get; private set; }

    public string? StepCode { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string? ArgsJson { get; private set; }

    /// <summary>Referência livre ao item tratado (ex.: nº do item, objeto, finding).</summary>
    public string? ItemRef { get; private set; }

    public static RunEvent Create(int runId, DateTimeOffset at, EventLevel level, string? step, string code, string? argsJson, string? itemRef = null) =>
        new()
        {
            RunId = runId, At = at, Level = level, StepCode = step, Code = Guard.Required(code, "RunEvent.CodeRequired", 100), ArgsJson = argsJson,
            ItemRef = Guard.Optional(itemRef, 200),
        };
}
