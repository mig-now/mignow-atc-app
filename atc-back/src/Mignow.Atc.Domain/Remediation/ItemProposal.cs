namespace Mignow.Atc.Domain.Remediation;

/// <summary>
/// Proposta de correção (por item ou grupo, desenho §3.4). <see cref="Origin"/> = <c>KB</c>|<c>IA</c>;
/// <see cref="Action"/> = <c>corrigir</c>|<c>isencao</c>|<c>cancelar</c>|<c>pendente</c>. Coluna jsonb.
/// </summary>
public sealed record ItemProposal(
    string Origin,
    string? KbCardId,
    IReadOnlyList<ProposalBlock> Blocks,
    string? MarkedBlock,
    string? NewSource,
    string? Diff,
    IReadOnlyList<string> Alerts,
    bool? SyntaxOk,
    IReadOnlyList<string>? SyntaxMessages,
    string? Model,
    int? Tokens,
    string? Justification,
    string Action,
    string? Review,
    DateTimeOffset? ProposedAt,
    IReadOnlyList<ProposalFactCandidate>? FactCandidates = null)
{
    public const string OriginKb = "KB";
    public const string OriginAi = "IA";
    public const string ActionFix = "corrigir";
    public const string ActionExemption = "isencao";
    public const string ActionCancel = "cancelar";
    public const string ActionPending = "pendente";
}

/// <summary>Bloco antes/depois. <see cref="Kind"/> = <c>modificacao</c>|<c>delecao</c>|<c>insercao</c>.</summary>
public sealed record ProposalBlock(IReadOnlyList<string> Before, IReadOnlyList<string> After, int? Line, string Kind, string? RelatedObject);

/// <summary>Fato técnico sugerido pela IA para este item (a gravação na KB é decisão do consultor, <c>facts</c> da decisão).</summary>
public sealed record ProposalFactCandidate(string Element, string Fact, string? Proof, string? Impact);
