using Mignow.Atc.Domain.Common;

namespace Mignow.Atc.Domain.Execution;

/// <summary>
/// Execução genérica de um módulo num ambiente (desenho §3.2). Uma execução ativa por ambiente: garantido por índice
/// único parcial no banco sobre <see cref="SapSystemId"/> enquanto <see cref="IsActive"/>. <see cref="ParametersJson"/>
/// guarda o pedido (o pipeline do <see cref="Kind"/> interpreta) e <see cref="ResultJson"/> o resumo (contagens).
/// </summary>
public sealed class Run : EntityBase
{
    public static readonly RunStatus[] ActiveStatuses = [RunStatus.Queued, RunStatus.Running, RunStatus.Cancelling];

    private readonly List<RunStep> _steps = [];

    private Run() { }

    public RunKind Kind { get; private set; }

    public int CustomerId { get; private set; }

    public int SapSystemId { get; private set; }

    /// <summary>Id público do alvo (sessão, lote de isenção, varredura, extração); nulo quando não há.</summary>
    public Guid? SubjectId { get; private set; }

    public string ParametersJson { get; private set; } = "{}";

    public string? ResultJson { get; private set; }

    public RunStatus Status { get; private set; }

    public string StartedBy { get; private set; } = string.Empty;

    public string? CancelledBy { get; private set; }

    public DateTimeOffset QueuedAt { get; private set; }

    public DateTimeOffset? StartedAt { get; private set; }

    public DateTimeOffset? FinishedAt { get; private set; }

    public DateTimeOffset? HeartbeatAt { get; private set; }

    public string? FailureCode { get; private set; }

    public string? FailureArgs { get; private set; }

    /// <summary>Tarefas em paralelo usadas (histórico do processo, desenho §3.8).</summary>
    public int Parallelism { get; private set; } = 1;

    public int AiInputTokens { get; private set; }

    public int AiOutputTokens { get; private set; }

    /// <summary>Soma do custo informado pelo serviço (US$); as chamadas sem valor ficam em <see cref="AiCallsWithoutCost"/>.</summary>
    public decimal AiCost { get; private set; }

    public int AiCallsWithoutCost { get; private set; }

    public int AiCalls { get; private set; }

    public IReadOnlyList<RunStep> Steps => _steps;

    public bool IsActive => ActiveStatuses.Contains(Status);

    public static Run Create(RunKind kind, int customerId, int sapSystemId, Guid? subjectId, string? parametersJson, string startedBy,
        IEnumerable<string> stepCodes, DateTimeOffset at)
    {
        if (customerId <= 0 || sapSystemId <= 0)
            throw new DomainException("Run.SapSystemRequired", "A execução precisa de cliente e ambiente.");
        var codes = stepCodes.ToList();
        if (codes.Count == 0)
            throw new DomainException("Run.NoSteps", "O pipeline não tem passos.");
        if (codes.Distinct(StringComparer.Ordinal).Count() != codes.Count)
            throw new DomainException("Run.DuplicateStep", "Passo repetido no pipeline.");
        var run = new Run
        {
            Kind = kind,
            CustomerId = customerId,
            SapSystemId = sapSystemId,
            SubjectId = subjectId,
            ParametersJson = string.IsNullOrWhiteSpace(parametersJson) ? "{}" : parametersJson,
            Status = RunStatus.Queued,
            StartedBy = Guard.Required(startedBy, "Run.StartedByRequired"),
            QueuedAt = at,
        };
        var order = 1;
        foreach (var code in codes)
            run._steps.Add(RunStep.Create(code, order++));
        return run;
    }

    public RunStep Step(string code) =>
        _steps.FirstOrDefault(s => s.Code == code) ?? throw new DomainException("Run.UnknownStep", $"Passo {code} não existe na execução.");

    public void Start(DateTimeOffset at)
    {
        if (Status != RunStatus.Queued)
            throw new DomainException("Run.NotQueued", "Só uma execução na fila pode iniciar.");
        Status = RunStatus.Running;
        StartedAt ??= at;
        HeartbeatAt = at;
    }

    public void Heartbeat(DateTimeOffset at) => HeartbeatAt = at;

    public void SetParallelism(int parallelism) => Parallelism = Math.Max(Parallelism, Math.Max(1, parallelism));

    /// <summary>Resumo do run (contagens). Gravado pelos passos; vai na resposta da API.</summary>
    public void SetResult(string? resultJson) => ResultJson = string.IsNullOrWhiteSpace(resultJson) ? null : resultJson;

    /// <summary>Interrupção pedida pelo consultor: o worker para num ponto seguro (nunca entre lock e unlock).</summary>
    public void RequestCancel(string by, DateTimeOffset at)
    {
        if (!IsActive)
            throw new DomainException("Run.NotActive", "A execução já terminou.");
        CancelledBy = by;
        if (Status == RunStatus.Queued)
        {
            MarkCancelled(at);
            return;
        }
        Status = RunStatus.Cancelling;
    }

    public void MarkCancelled(DateTimeOffset at)
    {
        Status = RunStatus.Cancelled;
        FinishedAt = at;
        foreach (var s in _steps)
            s.Skip(at);
    }

    public void Complete(DateTimeOffset at)
    {
        Status = RunStatus.Completed;
        FinishedAt = at;
    }

    public void Fail(DateTimeOffset at, string code, string? args = null)
    {
        Status = RunStatus.Failed;
        FinishedAt = at;
        FailureCode = code;
        FailureArgs = args;
        foreach (var s in _steps.Where(s => s.Status == StepStatus.Pending))
            s.Skip(at);
    }

    /// <summary>Retomada depois de queda do processo: volta para a fila; os passos concluídos não rodam de novo.</summary>
    public void Requeue()
    {
        if (Status != RunStatus.Running)
            return;
        Status = RunStatus.Queued;
        foreach (var s in _steps)
            s.ResetIfInterrupted();
    }

    /// <summary>Reprocessar a partir do passo que falhou (os passos são idempotentes); os concluídos ficam.</summary>
    public void Retry()
    {
        if (Status != RunStatus.Failed)
            throw new DomainException("Run.NotFailed", "Só uma execução com falha pode ser reprocessada.");
        Status = RunStatus.Queued;
        FinishedAt = null;
        FailureCode = FailureArgs = null;
        foreach (var s in _steps)
            s.ResetForRetry();
    }

    /// <summary>Custo nulo = o serviço não informou (fica fora da soma e conta em <see cref="AiCallsWithoutCost"/>).</summary>
    public void AddAiUsage(int inputTokens, int outputTokens, decimal? cost)
    {
        AiCalls++;
        AiInputTokens += inputTokens;
        AiOutputTokens += outputTokens;
        AiCost += cost ?? 0m;
        if (cost is null)
            AiCallsWithoutCost++;
    }
}
