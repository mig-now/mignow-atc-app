using System.Text.Json.Serialization;

namespace Mignow.Atc.Application.Modules.Remediation.Apply.Ai;

/// <summary>
/// Proposta de um item (formato do <c>propostas.json</c> do <c>paralelo_agente.md</c>): <c>acao</c> = corrigir | isencao | cancelar | pendente;
/// <c>origem</c> = KB | IA; <c>trechos</c> só para corrigir; <c>mesmo_bloco_de</c> = outro item da tarefa corrigido pelo mesmo trecho.
/// </summary>
public sealed record ProposalAiItem(
    [property: JsonPropertyName("n")] int N,
    [property: JsonPropertyName("acao")] string Action,
    [property: JsonPropertyName("origem")] string Origin,
    [property: JsonPropertyName("kb_card_id")] string? KbCardId,
    [property: JsonPropertyName("trechos")] IReadOnlyList<ProposalAiPatch>? Patches,
    [property: JsonPropertyName("justificativa")] string? Justification,
    [property: JsonPropertyName("provas")] string? Proof,
    [property: JsonPropertyName("fatos_candidatos")] IReadOnlyList<ProposalAiFact>? FactCandidates,
    [property: JsonPropertyName("mesmo_bloco_de")] int? SameBlockAs);
