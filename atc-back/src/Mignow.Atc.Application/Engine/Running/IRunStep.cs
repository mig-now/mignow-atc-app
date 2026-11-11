namespace Mignow.Atc.Application.Engine.Running;

/// <summary>
/// Um passo do pipeline (registrado por varredura, escopo da execução). Precisa ser IDEMPOTENTE: depois de uma queda
/// ou de um retry, o passo interrompido/falho roda de novo. Para no ponto seguro consultando
/// <see cref="RunContext.IsCancellationRequestedAsync"/> entre itens (nunca entre lock e unlock).
/// </summary>
public interface IRunStep
{
    /// <summary>Código estável do passo (o front traduz <c>steps.&lt;código&gt;</c>).</summary>
    string Code { get; }

    Task<StepOutcome> ExecuteAsync(RunContext context);
}
