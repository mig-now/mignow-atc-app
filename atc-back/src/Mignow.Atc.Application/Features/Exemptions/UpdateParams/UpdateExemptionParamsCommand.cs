using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Exemption;

namespace Mignow.Atc.Application.Features.Exemptions.UpdateParams;

/// <summary>
/// Parâmetros do pedido (Etapa 4 da skill): aprovador (usuário SAP ou área, diferente do solicitante), motivo (padrão <c>FPOS</c>),
/// validade (<c>U</c>/<c>D</c>/<c>P</c>) e prefixo do lote (nulo = padrão <c>{MOTIVO} - POS - ATC - MIGNOW | </c>; vazio = sem prefixo).
/// </summary>
public sealed record UpdateExemptionParamsCommand(Guid Id, string? Approver, string Reason, string Validity, string? JustificationPrefix = null)
    : ICommand<Result<ExemptionBatchResponse>>;
