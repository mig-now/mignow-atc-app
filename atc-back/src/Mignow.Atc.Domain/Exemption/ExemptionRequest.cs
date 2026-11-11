using Mignow.Atc.Domain.Common;

namespace Mignow.Atc.Domain.Exemption;

/// <summary>Pedido de isenção de um finding (escopo sempre <c>thisFinding</c>), com o XML enviado e o recebido.</summary>
public sealed class ExemptionRequest : EntityBase
{
    private ExemptionRequest() { }

    public int BatchId { get; private set; }

    public int ItemId { get; private set; }

    public string MarkerId { get; private set; } = string.Empty;

    public int Seq { get; private set; }

    public string Reason { get; private set; } = ExemptionBatch.DefaultReason;

    public string Justification { get; private set; } = string.Empty;

    public string? Status { get; private set; }

    /// <summary>Finding sem escopo <c>thisFinding</c> (checksum -1, ABAP Query): não é enviado.</summary>
    public bool ProbablyNoScope { get; private set; }

    public string? RequestXml { get; private set; }

    public string? ResponseXml { get; private set; }

    public string? Message { get; private set; }

    public static ExemptionRequest Create(int batchId, int itemId, string markerId, int seq, string reason, string justification, bool probablyNoScope) =>
        new()
        {
            BatchId = batchId,
            ItemId = itemId,
            MarkerId = Guard.Required(markerId, "ExemptionRequest.MarkerRequired", 300),
            Seq = seq,
            Reason = Guard.Required(reason, "ExemptionRequest.ReasonRequired", 10).ToUpperInvariant(),
            Justification = justification ?? string.Empty,
            ProbablyNoScope = probablyNoScope,
        };

    /// <summary>Motivo e justificativa do pedido (definidos na simulação; motivos diferentes por <c>seq</c> no mesmo lote).</summary>
    public void Prepare(string reason, string justification)
    {
        Reason = Guard.Required(reason, "ExemptionRequest.ReasonRequired", 10).ToUpperInvariant();
        Justification = justification ?? string.Empty;
    }

    public void Record(string status, string? message, string? requestXml = null, string? responseXml = null)
    {
        if (!ExemptionRequestStatus.IsValid(status))
            throw new DomainException("ExemptionRequest.InvalidStatus", $"Status inválido: {status}.");
        Status = status;
        Message = Guard.Optional(message, 2000);
        RequestXml = requestXml ?? RequestXml;
        ResponseXml = responseXml ?? ResponseXml;
    }
}
