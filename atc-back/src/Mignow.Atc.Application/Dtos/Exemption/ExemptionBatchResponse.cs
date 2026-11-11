using System.Text.Json;
using Mignow.Atc.Domain.Exemption;

namespace Mignow.Atc.Application.Dtos.Exemption;

/// <summary>Lote de isenção: parâmetros, descoberta (JSON), situação e totais por status dos pedidos (<c>PENDENTE</c> = ainda não simulado).</summary>
public sealed record ExemptionBatchResponse(
    Guid Id, Guid SessionId, Guid SapSystemId, string Priorities, string? Approver, string Reason, string Validity, string? JustificationPrefix,
    string EffectiveJustificationPrefix, JsonElement? Discovery, ExemptionBatchStatus Status, int Requests, IReadOnlyDictionary<string, int> Totals,
    ExemptionQuantitiesResponse Quantities, DateTimeOffset CreatedAt, string CreatedBy);
