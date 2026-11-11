namespace Mignow.Atc.Application.Modules.Remediation.Apply.Ai;

/// <summary>
/// Entrada da IA para propor correção (vai como JSON no prompt do usuário): um fonte físico, um objeto referenciado e os itens do ATC que
/// o usam nesse fonte. <see cref="Mode"/> = <c>kb</c> (aplicar o cartão da KB ao código do cliente) | <c>ia</c> (Análise da IA: desconsidera a KB).
/// <see cref="SourceKind"/> = <c>abap</c> | <c>ddl</c> (estrutura/tabela do dicionário).
/// </summary>
public sealed record ProposalTask(
    string Mode, string ObjectName, string ObjectType, string SourceName, string SourceKind, string Reference, string? ReferenceType,
    string? CallForm, IReadOnlyList<ProposalTaskItem> Items, IReadOnlyList<ProposalKbCard> KbCards, IReadOnlyList<ProposalFact> KbFacts,
    IReadOnlyList<ProposalUsage> Usages, IReadOnlyList<ProposalWindow> SourceWindows, IReadOnlyList<ProposalDdic> Ddic, IReadOnlyList<string> Prerequisites);
