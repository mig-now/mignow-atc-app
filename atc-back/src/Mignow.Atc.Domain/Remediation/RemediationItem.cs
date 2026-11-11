using Mignow.Atc.Domain.Common;

namespace Mignow.Atc.Domain.Remediation;

/// <summary>
/// Item da sessão (uma linha do CSV do ATC, <see cref="N"/> = número da linha). <see cref="Status"/> usa os textos exatos
/// da skill (<see cref="RemediationStatus"/>); as transições passam por <see cref="MarkStatus"/>, porte de
/// <c>marcar()</c> do atc.py: o histórico só ganha entrada quando o status muda.
/// </summary>
public sealed class RemediationItem : EntityBase
{
    private RemediationItem() { }

    public int SessionId { get; private set; }

    public int N { get; private set; }

    public string ObjectName { get; private set; } = string.Empty;

    public string ObjectType { get; private set; } = string.Empty;

    public string? RefObject { get; private set; }

    public string? RefType { get; private set; }

    public string? Check { get; private set; }

    public string? Message { get; private set; }

    public string? Note { get; private set; }

    public string? Text { get; private set; }

    public string? Package { get; private set; }

    /// <summary>Ambiguous | Functional | Technical (coluna Text do ATC).</summary>
    public string? Classification { get; private set; }

    public string? Priority { get; private set; }

    public string? FindingId { get; private set; }

    public string Origin { get; private set; } = ItemOrigin.Atc;

    /// <summary>Linha original completa do CSV (na ordem das colunas da sessão).</summary>
    public string RawRowJson { get; private set; } = "[]";

    public string Status { get; private set; } = RemediationStatus.Pending;

    public string Obs { get; private set; } = string.Empty;

    public DateTimeOffset? StatusDate { get; private set; }

    public IReadOnlyList<StatusChange> StatusHistory { get; private set; } = [];

    /// <summary>Regras de triagem que tocaram o item (R1..R9, MIGNOW_FSCM_PRODUCT…).</summary>
    public IReadOnlyList<string> TriageFlags { get; private set; } = [];

    public ItemPreparation? Preparation { get; private set; }

    public ItemProposal? Proposal { get; private set; }

    public ItemApplication? Application { get; private set; }

    /// <summary>
    /// Pendente de aprovação: o item já tem proposta esperando a decisão do consultor – correção que não passou nas travas do modo automático,
    /// sugestão de isenção/cancelamento ou "sem solução" da IA. Só "sem cartão na KB" (proposta KB pendente) não conta: ainda falta a IA.
    /// Coluna própria (a proposta é JSON) para filtrar e contar; vale enquanto o item estiver pendente (status vazio, como na skill).
    /// </summary>
    public bool AwaitingApproval { get; private set; }

    /// <summary>Cópia de <c>Application.Result</c> em coluna própria (a aplicação é JSON com o backup do fonte): filtro e contagem de falhas no SAP.</summary>
    public string? ApplicationResult { get; private set; }

    /// <summary>A "ficha" <c>correcoes/item_NNNN_&lt;obj&gt;.md</c>.</summary>
    public string? CorrectionSheetMd { get; private set; }

    public static RemediationItem Create(int sessionId, int n, string objectName, string objectType, string? refObject, string? refType,
        string? check, string? message, string? note, string? text, string? package, string? classification, string? priority,
        string? findingId, string origin, string rawRowJson)
    {
        if (n < 1)
            throw new DomainException("Item.InvalidLine", "Linha do CSV a partir de 1.");
        if (origin is not (ItemOrigin.Atc or ItemOrigin.Mignow))
            throw new DomainException("Item.InvalidOrigin", "Origem ATC ou MIGNOW.");
        return new RemediationItem
        {
            SessionId = sessionId,
            N = n,
            ObjectName = Guard.Required(objectName, "Item.ObjectRequired", 120),
            ObjectType = Guard.Required(objectType, "Item.ObjectTypeRequired", 10),
            RefObject = Guard.Optional(refObject, 200),
            RefType = Guard.Optional(refType, 10),
            Check = Guard.Optional(check, 300),
            Message = Guard.Optional(message, 500),
            Note = Guard.Optional(note, 40),
            Text = Guard.Optional(text, 500),
            Package = Guard.Optional(package, 60),
            Classification = Guard.Optional(classification, 20),
            Priority = Guard.Optional(priority, 5),
            FindingId = Guard.Optional(findingId, 300),
            Origin = origin,
            RawRowJson = string.IsNullOrWhiteSpace(rawRowJson) ? "[]" : rawRowJson,
        };
    }

    /// <summary>
    /// Porte de <c>marcar(it, status, obs, agora)</c>: quando o status muda, acrescenta {data, status} ao histórico; grava
    /// status, obs e data sempre.
    /// </summary>
    public void MarkStatus(string status, string? obs, DateTimeOffset at)
    {
        if (!RemediationStatus.IsValid(status))
            throw new DomainException("Item.InvalidStatus", $"Status inválido: {status}.");
        if (!string.Equals(status, Status, StringComparison.Ordinal))
            StatusHistory = [.. StatusHistory, new StatusChange(at, status)];
        Status = status;
        Obs = obs ?? string.Empty;
        StatusDate = at;
    }

    /// <summary>Reimportação: o item herda status/obs/histórico do mesmo nº de linha e objeto do ciclo anterior.</summary>
    public void CarryOver(string status, string obs, DateTimeOffset? statusDate, IReadOnlyList<StatusChange> history)
    {
        if (!RemediationStatus.IsValid(status))
            throw new DomainException("Item.InvalidStatus", $"Status inválido: {status}.");
        Status = status;
        Obs = obs;
        StatusDate = statusDate;
        StatusHistory = [.. history];
    }

    public void SetTriageFlags(IEnumerable<string> flags) => TriageFlags = [.. flags.Where(f => !string.IsNullOrWhiteSpace(f)).Distinct()];

    public void SetPreparation(ItemPreparation? preparation) => Preparation = preparation;

    public void SetProposal(ItemProposal? proposal)
    {
        Proposal = proposal;
        AwaitingApproval = proposal is not null && proposal is not { Origin: ItemProposal.OriginKb, Action: ItemProposal.ActionPending };
    }

    public void SetApplication(ItemApplication? application)
    {
        Application = application;
        ApplicationResult = application?.Result;
    }

    public void SetCorrectionSheet(string? markdown) => CorrectionSheetMd = markdown;
}
