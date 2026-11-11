using Microsoft.Extensions.Localization;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Application.Ports.Time;
using Mignow.Atc.Application.Resources;
using Mignow.Atc.Domain.Common;
using Mignow.Atc.Domain.Registry;
using Mignow.Atc.Domain.Remediation;

namespace Mignow.Atc.Application.Modules.Remediation;

/// <summary>
/// Criação de sessão a partir de linhas no layout do CSV do ATC (porte de <c>cmd_importar</c>). PONTO DE ENTRADA PÚBLICO para outros módulos:
/// o módulo Scan chama <see cref="CreateSessionFromRowsAsync"/> com <c>Source = MignowScan</c> (CSV P1 da varredura). Só adiciona ao
/// <see cref="IDataStore"/>; o commit é do TransactionDecorator do handler que chamou. Reimportar arquiva a(s) sessão(ões) anterior(es) do ambiente
/// (ciclo anterior, como o <c>backup_sessao_*</c>) e o novo ciclo herda status/obs/histórico dos itens de mesmo nº de linha e objeto.
/// </summary>
public sealed class SessionCreator(IDataStore store, IDateTimeProvider clock, IStringLocalizer<Errors> l)
{
    public async Task<Result<SessionCreated>> CreateSessionFromRowsAsync(SapSystem system, SessionSource source, CancellationToken ct)
    {
        var outcome = SessionImporter.Import(source.Document, source.Priorities);
        if (outcome.IsFailure)
            return l.Validation(outcome.ErrorCode!);
        var now = clock.UtcNow;
        var sessions = await store.ListAsync(store.Query<RemediationSession>().Where(s => s.SapSystemId == system.Id), ct);
        var previous = sessions.Where(s => !s.Archived).OrderByDescending(s => s.Cycle).FirstOrDefault();
        var previousItems = previous is null
            ? []
            : (await store.ListAsync(store.Read<RemediationItem>().Where(i => i.SessionId == previous.Id), ct)).ToDictionary(i => i.N);
        var items = new List<RemediationItem>();
        var carried = 0;
        var ixOrigin = AtcCsv.Col(source.Document.Header, "Origem", "Origin");
        foreach (var r in outcome.Rows)
        {
            try
            {
                var item = RemediationItem.Create(0, r.N, r.ObjectName, r.ObjectType, r.Ref, r.RefType, r.Check, r.Message, r.Note, r.Text, r.Package,
                    r.Classification, r.Priority, r.FindingId, OriginOf(source, ixOrigin, r), Json.Write(r.RawRow));
                if (previousItems.TryGetValue(r.N, out var old) && old.ObjectName == item.ObjectName)
                {
                    item.CarryOver(old.Status, old.Obs, old.StatusDate, old.StatusHistory);
                    carried++;
                }
                items.Add(item);
            }
            catch (DomainException)
            {
                // linha que não cabe no item (nome grande demais): fica fora, como as em branco
            }
        }
        var session = RemediationSession.Create(system.CustomerId, system.Id, source.Source, source.SourceRef, outcome.Summary!.Priorities,
            source.Request ?? system.DefaultRequest, system.Author, system.ProjectType, Json.Write(outcome.Header), source.Document.Encoding,
            source.Document.Delimiter, sessions.Count == 0 ? 1 : sessions.Max(s => s.Cycle) + 1);
        session.AddItems(items);
        var kept = items.Select(i => i.N).ToHashSet();
        session.SetExcludedRows(SessionExporter.ExcludedJson(
            source.Document.Rows.Select((row, k) => (N: k + 1, Row: row)).Where(x => !kept.Contains(x.N))));
        var archived = 0;
        foreach (var s in sessions.Where(s => !s.Archived))
        {
            s.Archive(now);
            archived++;
        }
        store.Add(session);
        return new SessionCreated(session, outcome.Summary with { Items = items.Count, IgnoredRows = outcome.Summary.IgnoredRows + outcome.Rows.Count - items.Count },
            carried, archived);
    }

    /// <summary>Varredura MIGNOW (fonte <c>MignowScan</c>) ou coluna "Origem" = MIGNOW na linha (CSV P1 da varredura) = <see cref="ItemOrigin.Mignow"/>; o resto é ATC.</summary>
    private static string OriginOf(SessionSource source, int? ixOrigin, ImportedRow row) =>
        source.Source == RemediationSource.MignowScan
        || (ixOrigin is { } k && k < row.RawRow.Count && string.Equals(row.RawRow[k].Trim(), "MIGNOW", StringComparison.OrdinalIgnoreCase))
            ? ItemOrigin.Mignow
            : ItemOrigin.Atc;
}
