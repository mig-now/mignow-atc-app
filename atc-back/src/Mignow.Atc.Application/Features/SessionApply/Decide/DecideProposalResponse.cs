namespace Mignow.Atc.Application.Features.SessionApply.Decide;

/// <summary>Itens decididos (status gravado), cartão criado na KB (ConfirmIa) e ids dos fatos registrados.</summary>
public sealed record DecideProposalResponse(IReadOnlyList<DecidedItem> Items, string? KbCardId, IReadOnlyList<string> FactIds);
