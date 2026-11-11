using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Exemption;

namespace Mignow.Atc.Application.Features.Exemptions.Create;

/// <summary>
/// Cria o lote (= <c>listar.py</c>): itens <c>Isenção</c> da sessão nas prioridades confirmadas (1..3). <c>IncludeObs</c> = sessões
/// antigas cuja observação pede isenção; <c>Items</c>/<c>Exclude</c> = números de linha a incluir/excluir.
/// </summary>
public sealed record CreateExemptionBatchCommand(Guid SessionId, IReadOnlyList<int> Priorities, bool IncludeObs = false, IReadOnlyList<int>? Items = null,
    IReadOnlyList<int>? Exclude = null) : ICommand<Result<ExemptionBatchCreatedResponse>>;
