using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Domain.Extraction;

namespace Mignow.Atc.Application.Modules.Extraction;

/// <summary>Consultas de extrações gravadas (só as completas; a que ficou pela metade não conta).</summary>
public static class ExtractionQueries
{
    /// <summary>A extração mais recente desta execução do ATC no ambiente (nunca se extrai duas vezes, salvo force).</summary>
    public static Task<AtcExtraction?> FindByDisplayIdAsync(IDataStore store, int sapSystemId, string displayId, bool tracked, CancellationToken ct)
    {
        var q = tracked ? store.Query<AtcExtraction>() : store.Read<AtcExtraction>();
        return store.FirstOrDefaultAsync(
            q.Where(e => e.SapSystemId == sapSystemId && e.DisplayId == displayId && e.IsComplete).OrderByDescending(e => e.ExtractedAt).ThenByDescending(e => e.Id), ct);
    }

    /// <summary>A extração anterior para a variação: a mais recente de OUTRA execução do ATC (o "anterior" do <c>resultado.py</c>).</summary>
    public static Task<AtcExtraction?> FindPreviousAsync(IDataStore store, int sapSystemId, string displayId, CancellationToken ct) =>
        store.FirstOrDefaultAsync(
            store.Read<AtcExtraction>().Where(e => e.SapSystemId == sapSystemId && e.DisplayId != displayId && e.IsComplete)
                .OrderByDescending(e => e.ExtractedAt).ThenByDescending(e => e.Id), ct);
}
