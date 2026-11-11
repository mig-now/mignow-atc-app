namespace Mignow.Atc.Application.Engine.RemediationApply;

/// <summary>Parâmetros dos runs <c>ApplySimulate</c> e <c>Apply</c>: itens e, para tabela, o aval do consultor à mudança de estrutura (<c>-PermitirEstrutura</c>).</summary>
public sealed record ApplyParameters(Guid SessionId, IReadOnlyList<Guid> ItemIds, bool AllowTableStructure, IReadOnlyList<DdicCreateParameter>? DdicCreate);
