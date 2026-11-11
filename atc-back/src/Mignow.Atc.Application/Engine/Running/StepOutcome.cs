namespace Mignow.Atc.Application.Engine.Running;

/// <summary>
/// Resultado do passo. <see cref="StopRun"/> encerra a execução como falha (o retry recomeça neste passo);
/// <see cref="Cancelled"/> encerra como cancelada (o consultor pediu e o passo parou num ponto seguro).
/// </summary>
public sealed record StepOutcome(bool StopRun, string? MessageCode = null, string? MessageArgs = null, bool Cancelled = false)
{
    public static readonly StepOutcome Done = new(false);

    public static readonly StepOutcome Cancel = new(false, Cancelled: true);

    public static StepOutcome Info(string code, string? args = null) => new(false, code, args);

    public static StepOutcome Stop(string code, string? args = null) => new(true, code, args);
}
