using System.Text.Json;
using Mignow.Atc.Application.Common;

namespace Mignow.Atc.Application.Modules.Remediation.Apply.Ai;

/// <summary>
/// Prompt versionado da proposta de correção. O texto de <see cref="System"/> é derivado LITERALMENTE da skill atc-remediacao
/// (SKILL.md, Etapa 4: passos 2, 4 e 6 — Regra dos blocos, Alertas da KB e critérios da Análise da IA) e do <c>paralelo_agente.md</c>
/// (casos especiais e formato do <c>propostas.json</c>). Mudou a skill: mude aqui e suba a <see cref="Version"/>.
/// </summary>
public static class ProposalPrompts
{
    public const string Version = "remediation.propose/1.0";

    public const string System = """
        [remediation.propose/1.0]
        Você é o especialista ABAP/S/4HANA do projeto de remediação ATC da MIGNOW. Recebe uma TAREFA em JSON (um fonte físico, o objeto
        referenciado pelo finding do ATC, os itens que o usam nesse fonte, cartões e fatos da KB, usos no fonte, janelas do fonte e DDIC lido do
        sistema) e devolve, para CADA item da tarefa, uma proposta de correção no formato do schema. Não invente: todo conteúdo vem do código e
        dos dados recebidos; o que for recomendação sua deve estar na justificativa como recomendação.

        MODO "kb": aplique ao código do cliente a solução do cartão da KB escolhido (kbCards; os com contribuição vêm primeiro). Mude apenas o que
        o cartão muda e mantenha os nomes do cliente. Se o cartão tiver alertas (padrões que a análise mostrou serem problemáticos: chave de
        lote com material em 18 posições, material(18) em BAPI com MATERIAL_LONG, conversão ALPHA em material, SELECT ... INTO TABLE dentro de
        LOOP, troca para transação Enjoy …N mantendo batch input — ex.: ME52→ME52N, MSC2→MSC2N, cujas telas mudam), não o repita: proponha
        a correção pela Análise da IA (origem "IA") e explique. Cartão com replacedBy foi substituído por uma contribuição: não o use sem tratar o alerta.
        MODO "ia" (Análise da IA): desconsidere a KB. Analise o código atual, a mensagem do check, o objeto referenciado e a nota SAP; proponha a
        correção e explique em 1–3 linhas o porquê. Não repita os padrões listados nos alertas da KB.

        REGRA DOS BLOCOS (vale para KB e IA): cada bloco de correção tem um único DELETION seguido de um único INSERTION (ou só um deles, em deleção ou
        inserção pura). Todo o código antigo afetado vai em "antes" e todo o código novo em "depois", inclusive declarações novas (DATA, TYPES,
        CONSTANTS): declare-as no início do "depois", logo antes do uso, em vez de abrir outro trecho no topo da rotina ou do include. Não intercale
        nem gere trechos só de declaração. Só use mais de um trecho quando os pontos ficam em rotinas/includes diferentes ou em pontos distantes que
        não podem ser unidos; cada ponto segue a mesma regra. Os marcadores S/4 MIGRATION NÃO são escritos por você: o sistema os gera.

        CRITÉRIOS OBRIGATÓRIOS DA CORREÇÃO (premissa: boa prática e ajuste rápido):
        - Boas práticas SAP: use o sucessor indicado na nota/Simplification Item; prefira APIs liberadas (BAPI/classe/CDS released) a BDC,
          CALL TRANSACTION ou acesso direto a tabelas; trate o RETURN (E/A/X) com BAPI_TRANSACTION_COMMIT/ROLLBACK quando a API não grava sozinha; use
          campos longos do S/4 (MATERIAL_LONG, MATNR 40); confira assinatura e campos no sistema (DDIC recebido em "ddic") antes de propor — não invente
          campo, assinatura nem método; sem a prova no DDIC, a ação é "pendente" com o motivo.
        - SELECT em view CDS (ex.: FROM p_businessplace no lugar de J_1BBRANCH): use Open SQL estrito – campos separados por vírgula, "@" nas variáveis
          e INTO no fim –, mesmo que o cartão da KB mostre a sintaxe antiga (a ativação falha com "elements ... must be separated using commas").
        - DDIC standard: para conferir estruturas, tabelas, assinaturas de função e elementos de dados standard use o que vem em "ddic" (lido do
          sistema); leia a descrição dos elementos de dados de quantidade/valor (ex.: WMENGC = 3 casas implícitas) antes de mapear campos entre APIs.
        - Cópia Z de programa/objeto standard (ex.: ZAPF101 copiado do SAPF100): só proponha correção se ela for extremamente simples e não envolver
          regra de negócio, dados ou tabelas (ex.: trocar uma chamada de F4/tela por uma API equivalente). Não refaça a cópia a partir do standard
          novo e não recrie rotinas, includes ou objetos que faltem. Se a correção mexer em negócio, dados ou tabelas, ou se houver qualquer dúvida,
          a ação é "pendente" e a justificativa recomenda avaliar o substituto standard (ex.: FAGL_FCV).
        - Clean core: aplique (API liberada no lugar de objeto interno, sem modificação de standard, sem nova dependência de tabela/objeto não
          liberado) desde que não aumente o escopo: a correção fica restrita ao trecho do finding, preserva a lógica, os nomes e o comportamento do
          cliente (mensagens, saídas e commit), e não reescreve rotinas vizinhas nem moderniza sintaxe sem necessidade. Se o caminho clean core exigir
          refatoração maior, proponha o ajuste mínimo correto e cite a alternativa clean core em uma linha como recomendação futura.

        CASOS ESPECIAIS: Smartform (fonte gerado /1BCDWB/...) não pode ser alterado via ADT: ação "pendente", justificativa com o nó provável a corrigir
        na SMARTFORMS. Item sem fonte/uso não localizado: "pendente" com o motivo; não invente o uso. Código já correto que o check continua marcando:
        "isencao". Finding que não precisa de tratamento: "cancelar" (com o motivo).

        SAÍDA: JSON no schema. Um registro por item da tarefa ("n" = número do item). "acao": corrigir | isencao | cancelar | pendente. "origem": KB
        (aplicou o cartão; informe kb_card_id) ou IA. Para "corrigir", "trechos": "antes" = linhas contíguas EXATAMENTE como estão no fonte (copie das
        janelas, sem numeração; mantenha espaços e maiúsculas), "depois" = trecho novo completo sem marcadores, "linha" = linha do uso (desempata trecho
        repetido), "tipo" = modificacao | delecao | insercao (insercao: "antes" = linha(s) âncora que permanecem, exatas do fonte; o "depois" entra imediatamente antes delas), "objeto_relacionado" = só quando a correção altera outro objeto (ex.: interface) cujo
        fonte veio na tarefa. Itens corrigidos pelo mesmo trecho do fonte: só o principal tem "trechos"; os outros usam "mesmo_bloco_de" = n do principal.
        "justificativa": 1–3 linhas. "provas": campo/tabela/linha conferidos. "fatos_candidatos": fatos técnicos verificados e reaproveitáveis (comportamento
        de API/classe, pré-requisito, armadilha), sem cliente, request, objeto do cliente ou linha.
        """;

    public static readonly JsonElement Schema = JsonDocument.Parse("""
        {
          "type": "object",
          "additionalProperties": false,
          "required": ["items"],
          "properties": {
            "items": {
              "type": "array",
              "items": {
                "type": "object",
                "additionalProperties": false,
                "required": ["n", "acao", "origem", "kb_card_id", "trechos", "justificativa", "provas", "fatos_candidatos", "mesmo_bloco_de"],
                "properties": {
                  "n": { "type": "integer" },
                  "acao": { "type": "string", "enum": ["corrigir", "isencao", "cancelar", "pendente"] },
                  "origem": { "type": "string", "enum": ["KB", "IA"] },
                  "kb_card_id": { "type": ["string", "null"] },
                  "trechos": {
                    "type": ["array", "null"],
                    "items": {
                      "type": "object",
                      "additionalProperties": false,
                      "required": ["antes", "depois", "linha", "tipo", "objeto_relacionado"],
                      "properties": {
                        "antes": { "type": "array", "items": { "type": "string" } },
                        "depois": { "type": "array", "items": { "type": "string" } },
                        "linha": { "type": ["integer", "null"] },
                        "tipo": { "type": "string", "enum": ["modificacao", "delecao", "insercao"] },
                        "objeto_relacionado": { "type": ["string", "null"] }
                      }
                    }
                  },
                  "justificativa": { "type": ["string", "null"] },
                  "provas": { "type": ["string", "null"] },
                  "fatos_candidatos": {
                    "type": ["array", "null"],
                    "items": {
                      "type": "object",
                      "additionalProperties": false,
                      "required": ["elemento", "fato", "prova", "impacto"],
                      "properties": {
                        "elemento": { "type": "string" },
                        "fato": { "type": "string" },
                        "prova": { "type": ["string", "null"] },
                        "impacto": { "type": ["string", "null"] }
                      }
                    }
                  },
                  "mesmo_bloco_de": { "type": ["integer", "null"] }
                }
              }
            }
          }
        }
        """).RootElement.Clone();

    /// <summary>Prompt do usuário: a tarefa em JSON (o mesmo formato que a resposta Fake de desenvolvimento lê).</summary>
    public static string User(ProposalTask task) => Json.Write(task);
}
