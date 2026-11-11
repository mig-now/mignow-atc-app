using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Features.SessionApply;

/// <summary>Sessão pelo id público, o ambiente (com o cliente) e os itens pelos ids públicos.</summary>
public static class SessionApplyLoader
{
    public static Task<RemediationSession?> SessionAsync(IDataStore store, Guid id, bool tracked, CancellationToken ct) =>
        store.FirstOrDefaultAsync((tracked ? store.Query<RemediationSession>() : store.Read<RemediationSession>()).Where(s => s.Guid == id), ct);

    public static Task<SapSystem?> SystemAsync(IDataStore store, int systemId, CancellationToken ct) =>
        store.FirstOrDefaultAsync(store.Including(store.Read<SapSystem>(), nameof(SapSystem.Customer)).Where(s => s.Id == systemId), ct);

    public static Task<List<RemediationItem>> ItemsAsync(IDataStore store, int sessionId, IReadOnlyCollection<Guid> ids, bool tracked, CancellationToken ct) =>
        store.ListAsync((tracked ? store.Query<RemediationItem>() : store.Read<RemediationItem>()).Where(i => i.SessionId == sessionId && ids.Contains(i.Guid)).OrderBy(i => i.N), ct);
}
