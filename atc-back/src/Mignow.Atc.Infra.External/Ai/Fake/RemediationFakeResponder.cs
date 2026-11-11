using System.Text.Json;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Modules.Remediation.Apply.Ai;
using Mignow.Atc.Application.Ports.Ai;

namespace Mignow.Atc.Infra.External.Ai.Fake;

/// <summary>
/// IA de desenvolvimento do módulo Remediação (determinística, sem custo): <c>remediation.propose</c> lê a tarefa em JSON do prompt e troca, na linha do uso,
/// o objeto referenciado por <c>Z_&lt;objeto&gt;</c> (origem KB quando há cartão sem alerta; senão IA); itens na mesma linha viram <c>mesmo_bloco_de</c>;
/// sem a linha no fonte, a proposta é <c>pendente</c>. <c>remediation.review</c> devolve um parecer <c>ok</c>.
/// </summary>
public sealed class RemediationFakeResponder : IFakeAiResponder
{
    public IReadOnlyCollection<string> Purposes { get; } = [RemediationPurposes.Propose, RemediationPurposes.Review];

    public string Respond(AiRequest request) => request.Purpose == RemediationPurposes.Review ? Review() : Propose(request);

    private static string Review() => JsonSerializer.Serialize(new
    {
        veredito = "ok", resumo = "Revisão simulada (IA de desenvolvimento): troca pontual, sem impacto aparente.", evidencias = new[] { "diff restrito ao trecho do finding" },
        riscos = Array.Empty<string>(), testes = new[] { "Executar o programa com dados de teste" }, validacao_humana = Array.Empty<string>(),
    });

    private static string Propose(AiRequest request)
    {
        var task = JsonSerializer.Deserialize<ProposalTask>(request.User, Json.Options)!;
        var lines = task.SourceWindows.SelectMany(w => w.Text.Split('\n').Select((t, i) => (Number: w.FromLine + i, Text: t))).ToList();
        var card = task.KbCards.FirstOrDefault(c => c.Alerts.Count == 0 && c.ReplacedBy is null);
        var byLine = new Dictionary<int, int>();
        List<object> items = [];
        foreach (var it in task.Items)
        {
            var target = it.Line is { } l ? lines.Where(x => x.Text.Contains(task.Reference, StringComparison.OrdinalIgnoreCase)).OrderBy(x => Math.Abs(x.Number - l)).FirstOrDefault() : default;
            if (target.Text is null || task.Reference.Length == 0)
            {
                items.Add(new { n = it.N, acao = "pendente", origem = card is null ? "IA" : "KB", kb_card_id = (string?)null, trechos = (object?)null, justificativa = "Uso não localizado no fonte (resposta simulada).",
                    provas = (string?)null, fatos_candidatos = (object?)null, mesmo_bloco_de = (int?)null });
                continue;
            }
            if (byLine.TryGetValue(target.Number, out var main))
            {
                items.Add(new { n = it.N, acao = "corrigir", origem = card is null ? "IA" : "KB", kb_card_id = card?.Id, trechos = (object?)null, justificativa = "Mesmo trecho (resposta simulada).",
                    provas = (string?)null, fatos_candidatos = (object?)null, mesmo_bloco_de = (int?)main });
                continue;
            }
            byLine[target.Number] = it.N;
            var replaced = System.Text.RegularExpressions.Regex.Replace(target.Text, System.Text.RegularExpressions.Regex.Escape(task.Reference), "z_" + task.Reference.ToLowerInvariant(),
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            items.Add(new
            {
                n = it.N, acao = "corrigir", origem = card is null ? "IA" : "KB", kb_card_id = card?.Id,
                trechos = new[] { new { antes = new[] { target.Text }, depois = new[] { replaced }, linha = (int?)target.Number, tipo = "modificacao", objeto_relacionado = (string?)null } },
                justificativa = $"Troca de {task.Reference} (resposta simulada).", provas = (string?)null, fatos_candidatos = Array.Empty<object>(), mesmo_bloco_de = (int?)null,
            });
        }
        return JsonSerializer.Serialize(new { items });
    }
}
