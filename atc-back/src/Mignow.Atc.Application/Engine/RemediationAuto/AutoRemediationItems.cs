using Mignow.Atc.Application.Engine.Running;
using Mignow.Atc.Application.Modules.Remediation;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Engine.RemediationAuto;

/// <summary>Itens do play: os escolhidos (ou todos) da sessão, rastreados pelo contexto da execução. Standard (SAP) nunca entra.</summary>
internal static class AutoRemediationItems
{
    public static Task<(RemediationSession Session, List<RemediationItem> All, List<RemediationItem> Selected)?> LoadAsync(RunContext ctx, AutoRemediationParameters p) =>
        ctx.WithDbAsync<(RemediationSession, List<RemediationItem>, List<RemediationItem>)?>(async () =>
        {
            var session = await ctx.Store.FirstOrDefaultAsync(ctx.Store.Query<RemediationSession>().Where(s => s.Guid == p.SessionId), ctx.Ct);
            if (session is null)
                return null;
            var id = session.Id;
            var all = await ctx.Store.ListAsync(ctx.Store.Query<RemediationItem>().Where(i => i.SessionId == id).OrderBy(i => i.N), ctx.Ct);
            var ids = p.ItemIds?.ToHashSet();
            var selected = all.Where(i => ids is null || ids.Contains(i.Guid)).ToList();
            return (session, all, selected);
        });

    public static bool IsStandard(RemediationItem i) => StandardObjects.IsStandard(i.ObjectName);
}
