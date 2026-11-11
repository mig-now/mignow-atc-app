using Mignow.Atc.Domain.Common;

namespace Mignow.Atc.Domain.Exemption;

/// <summary>Lote de pedidos de isenção dos itens <c>Isenção</c> de uma sessão (skill atc-isencao).</summary>
public sealed class ExemptionBatch : EntityBase
{
    public const string DefaultReason = "FPOS";
    public const string DefaultValidity = "U";
    public const string DefaultJustificationPrefix = "{MOTIVO} - POS - ATC - MIGNOW | ";

    private readonly List<ExemptionRequest> _requests = [];

    private ExemptionBatch() { }

    /// <summary>Pedidos do lote (um por finding), na ordem do <c>seq</c>; só vêm se a consulta der Include.</summary>
    public IReadOnlyList<ExemptionRequest> Requests => _requests;

    public int SessionId { get; private set; }

    public int SapSystemId { get; private set; }

    public string Priorities { get; private set; } = string.Empty;

    public string? Approver { get; private set; }

    public string Reason { get; private set; } = DefaultReason;

    public string Validity { get; private set; } = DefaultValidity;

    /// <summary>
    /// Prefixo do lote antes da justificativa (<c>-Prefixo</c> do solicitar.ps1); <c>{MOTIVO}</c> vira o código do motivo.
    /// Nulo = padrão da skill (<see cref="DefaultJustificationPrefix"/>); vazio = sem prefixo. Não é aparado: o espaço
    /// final (<c>"| "</c>) faz parte do texto.
    /// </summary>
    public string? JustificationPrefix { get; private set; }

    /// <summary>Política, motivos e aprovadores descobertos no SAP (JSON).</summary>
    public string? DiscoveryJson { get; private set; }

    public ExemptionBatchStatus Status { get; private set; }

    public static ExemptionBatch Create(int sessionId, int sapSystemId, string priorities, string? approver, string? justificationPrefix) =>
        new()
        {
            SessionId = sessionId,
            SapSystemId = sapSystemId,
            Priorities = Guard.Required(priorities, "ExemptionBatch.PrioritiesRequired", 20),
            Approver = Guard.Optional(approver, 40)?.ToUpperInvariant(),
            JustificationPrefix = Prefix(justificationPrefix),
            Status = ExemptionBatchStatus.Created,
        };

    /// <summary>Acrescenta o pedido de um finding (<c>seq</c> = ordem no lote, a partir de 1; o markerId é único no lote).</summary>
    public ExemptionRequest AddRequest(int itemId, string markerId, int seq, bool probablyNoScope)
    {
        if (seq < 1)
            throw new DomainException("ExemptionRequest.InvalidSeq", "Seq a partir de 1.");
        var marker = markerId?.Trim() ?? string.Empty;
        if (_requests.Any(r => r.MarkerId == marker))
            throw new DomainException("ExemptionRequest.DuplicateMarker", $"O finding {marker} já está no lote.");
        var request = ExemptionRequest.Create(Id, itemId, markerId!, seq, Reason, string.Empty, probablyNoScope);
        _requests.Add(request);
        return request;
    }

    public void SetParameters(string? approver, string reason, string validity, string? justificationPrefix)
    {
        Approver = Guard.Optional(approver, 40)?.ToUpperInvariant();
        Reason = Guard.Required(reason, "ExemptionBatch.ReasonRequired", 10).ToUpperInvariant();
        Validity = Guard.Required(validity, "ExemptionBatch.ValidityRequired", 10).ToUpperInvariant();
        JustificationPrefix = Prefix(justificationPrefix);
    }

    public void SetDiscovery(string? discoveryJson) => DiscoveryJson = discoveryJson;

    /// <summary>Prefixo efetivo para um motivo (padrão da skill quando não informado; <c>{MOTIVO}</c> em maiúsculas).</summary>
    public string EffectivePrefix(string reason) =>
        (JustificationPrefix ?? DefaultJustificationPrefix).Replace("{MOTIVO}", reason.ToUpperInvariant(), StringComparison.Ordinal);

    public void MoveTo(ExemptionBatchStatus status) => Status = status;

    /// <summary>Só avança no fluxo (listar → descobrir → simular → enviar → conferir); nunca volta uma etapa.</summary>
    public void Advance(ExemptionBatchStatus status)
    {
        if (status > Status)
            Status = status;
    }

    private static string? Prefix(string? value) => value is null ? null : value.Length > 200 ? value[..200] : value;
}
