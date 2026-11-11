using Mignow.Atc.Application.Engine.RemediationApply;

namespace Mignow.Atc.Api.Contracts;

/// <summary>Corpo de <c>apply-simulate</c> e <c>apply</c>: itens; para tabela, o aval à mudança de estrutura; para objeto DDIC novo, pacote e descrição.</summary>
public sealed record SessionApplyItemsRequest(IReadOnlyList<Guid> ItemIds, bool AllowTableStructure = false, IReadOnlyList<DdicCreateParameter>? DdicCreate = null);
