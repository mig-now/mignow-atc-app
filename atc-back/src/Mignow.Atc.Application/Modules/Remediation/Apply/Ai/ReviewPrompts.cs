using System.Text.Json;
using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Modules.Remediation.Apply.Ai;

/// <summary>
/// Prompt versionado da revisão antes de gravar, derivado do agente <c>sap-atc-abap</c> (.claude/agents/sap-atc-abap.md): responsabilidade,
/// procedimento, itens de origem MIGNOW, restrições e formato de saída.
/// </summary>
public static class ReviewPrompts
{
    public const string Version = "remediation.review/1.0";

    public const string System = """
        [remediation.review/1.0]
        Você é o especialista técnico SAP ABAP (SAP ATC ABAP Specialist). Responsabilidade: analisar tecnicamente a correção proposta para um finding
        do ATC e revisá-la ANTES de ser gravada no sistema. Você recebe em JSON o finding, a proposta (origem KB ou IA, justificativa, alertas), o
        resultado da verificação de sintaxe sem gravar e o diff unificado.

        Procedimento: 1. identifique a mensagem e a regra ATC envolvidas; 2. analise o código afetado e seu contexto; 3. verifique a compatibilidade
        com a versão SAP alvo; 4. identifique impactos em chamadas, interfaces, estruturas e dependências; 5. confirme que a correção é a menor
        tecnicamente adequada e preserva a lógica, os nomes e o comportamento do cliente; 6. defina como a correção deve ser validada.

        Itens de origem MIGNOW (Origem = MIGNOW; Check Title iniciado por "MIGNOW REGEX_") não são findings do ATC: vêm da varredura regex da skill
        atc-extracao-mignow; a regra diz o que foi encontrado. Não têm Finding ID (não são candidatos à isenção ATC).

        Restrições: não afirme que uma correção foi aplicada sem evidência; não invente APIs, métodos ou resultados de testes; peça contexto adicional
        quando o finding for insuficiente; encaminhe impactos funcionais relevantes para análise funcional (cite em validacao_humana).

        Saída (JSON no schema): veredito = "ok" (pode gravar), "atencao" (pode gravar, com os pontos de atenção) ou "bloquear" (não deve ser gravado
        sem correção: erro de sintaxe, API inexistente, perda de lógica, regra dos blocos violada, correção que mexe em negócio sem validação);
        resumo (finding e causa provável + proposta, 1–3 linhas); evidencias encontradas no diff/entrada; riscos técnicos; testes necessários;
        validacao_humana = pontos que precisam de validação humana.
        """;

    public static readonly JsonElement Schema = JsonDocument.Parse("""
        {
          "type": "object",
          "additionalProperties": false,
          "required": ["veredito", "resumo", "evidencias", "riscos", "testes", "validacao_humana"],
          "properties": {
            "veredito": { "type": "string", "enum": ["ok", "atencao", "bloquear"] },
            "resumo": { "type": "string" },
            "evidencias": { "type": "array", "items": { "type": "string" } },
            "riscos": { "type": "array", "items": { "type": "string" } },
            "testes": { "type": "array", "items": { "type": "string" } },
            "validacao_humana": { "type": "array", "items": { "type": "string" } }
          }
        }
        """).RootElement.Clone();

    public static string User(ReviewTask task) => Json.Write(task);

    /// <summary>Texto do parecer gravado em <c>Proposal.Review</c> (markdown curto, com o veredito na primeira linha).</summary>
    public static string Render(ReviewAiResponse r)
    {
        static string List(string title, IReadOnlyList<string> items) => items.Count == 0 ? string.Empty : $"\n\n**{title}**\n" + string.Join('\n', items.Select(i => "- " + i));
        return $"**Veredito:** {r.Verdict}\n\n{r.Summary}{List("Evidências", r.Evidence)}{List("Riscos técnicos", r.Risks)}{List("Testes necessários", r.Tests)}{List("Validação humana", r.HumanValidation)}";
    }

    /// <summary>Veredito gravado na primeira linha do parecer (<c>**Veredito:** x</c>); nulo se não for um parecer da revisão.</summary>
    public static string? VerdictOf(string? review)
    {
        const string prefix = "**Veredito:** ";
        if (review is null || !review.StartsWith(prefix, StringComparison.Ordinal))
            return null;
        var end = review.IndexOf('\n', StringComparison.Ordinal);
        return (end < 0 ? review[prefix.Length..] : review[prefix.Length..end]).Trim();
    }
}
