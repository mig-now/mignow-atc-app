using System.Text;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Modules.Remediation.Apply;

/// <summary>
/// Relatório do lote do modo sem confirmação (SKILL.md: fluxo seguido, cada decisão tomada sem confirmação, itens aplicados no SAP com request,
/// resultado e backup, paradas e pendências para o consultor, riscos). Markdown, gravado no <c>ResultJson</c> do run.
/// </summary>
public static class BatchReportBuilder
{
    public static string Build(RemediationSession session, IReadOnlyList<RemediationItem> items, ProposeSummary? summary, IReadOnlyList<ApplyItemReport> applied, DateTimeOffset at)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# Relatório do lote – modo sem confirmação ({at:yyyy-MM-dd HH:mm})").AppendLine();
        sb.AppendLine($"Sessão ciclo {session.Cycle} · request {session.Request ?? "(não definida)"} · autor {ChangeMarker.AuthorTag(session.Author)}").AppendLine();
        sb.AppendLine("## Fluxo seguido").AppendLine("1. Proposta pela KB (Propose, mode=kb) · 2. Confirmação automática de KB sem alerta · 3. Simulação e revisão por IA · 4. Aplicação no SAP em fila com a request da sessão.").AppendLine();
        if (summary is not null)
            sb.AppendLine($"Proposta: {summary.Items} itens · corrigir {summary.Fix} · isenção {summary.Exemption} · cancelar {summary.Cancel} · pendente {summary.Pending} · KB {summary.FromKb} · IA {summary.FromAi} · com alerta {summary.WithAlerts}").AppendLine();
        var auto = items.Where(i => i.Obs.Contains("modo automático", StringComparison.Ordinal)).ToList();
        sb.AppendLine("## Decisões tomadas sem confirmação");
        if (auto.Count == 0)
            sb.AppendLine("Nenhuma.");
        foreach (var i in auto)
            sb.AppendLine($"- item {i.N} {i.ObjectType} {i.ObjectName}: {i.Status} – {i.Obs}");
        sb.AppendLine().AppendLine("## Aplicados no SAP");
        if (applied.Count == 0)
            sb.AppendLine("Nenhum.");
        foreach (var r in applied)
        {
            var it = items.FirstOrDefault(i => i.N == r.N);
            sb.AppendLine($"- item {r.N} {r.ObjectName}: {r.Result} · request {it?.Application?.Request} · backup {(it?.Application?.BackupSource is null ? "não" : "guardado no item")}{(r.Detail is null ? string.Empty : " · " + r.Detail)}");
        }
        sb.AppendLine().AppendLine("## Paradas e pendências para o consultor");
        var stops = items.Where(i => i.Status == RemediationStatus.Pending || i.Application?.Result is not (null or ApplyResults.Activated)).ToList();
        if (stops.Count == 0)
            sb.AppendLine("Nenhuma.");
        foreach (var i in stops)
        {
            var why = i.Proposal is null ? "sem proposta"
                : i.Proposal.Action != ItemProposal.ActionFix ? $"proposta {i.Proposal.Action}: {i.Proposal.Justification}"
                : i.Proposal.Alerts.Count > 0 ? $"cartão {i.Proposal.KbCardId} com alerta ({string.Join(" | ", i.Proposal.Alerts)}) – KB descartada para decisão do consultor"
                : i.Proposal.Origin == ItemProposal.OriginAi ? "proposta da IA: exige confirmação do consultor"
                : i.Application?.Result is { } r ? $"aplicação: {r}" : "aguardando decisão";
            sb.AppendLine($"- item {i.N} {i.ObjectName}: {why}");
        }
        return sb.ToString();
    }
}
