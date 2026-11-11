using Microsoft.EntityFrameworkCore;
using Mignow.Atc.Application.Ports.Execution;
using Mignow.Atc.Application.Ports.Time;
using Mignow.Atc.Domain.Execution;
using Mignow.Atc.Infra.Database.Context;

namespace Mignow.Atc.Infra.Database.Queue;

/// <summary>
/// Fila no Postgres com <c>FOR UPDATE SKIP LOCKED</c> (várias instâncias não pegam a mesma), <b>justa por cliente</b>: a vaga
/// vai primeiro para o cliente com menos execuções rodando e, entre esses, para a mais antiga. Assim um lote grande de um
/// cliente não segura os outros: clientes diferentes rodam ao mesmo tempo. Fora do Postgres (testes com InMemory),
/// mesma ordem pelo domínio, sem garantia de concorrência.
/// </summary>
public sealed class PostgresRunQueue(AtcDbContext db, IDateTimeProvider clock) : IRunQueue
{
    public async Task<int?> ClaimNextAsync(CancellationToken ct)
    {
        if (!db.Database.IsNpgsql())
        {
            var busy = new[] { RunStatus.Running, RunStatus.Cancelling };
            var run = await db.Set<Run>().Where(r => r.Status == RunStatus.Queued)
                .OrderBy(r => db.Set<Run>().Count(x => x.CustomerId == r.CustomerId && busy.Contains(x.Status)))
                .ThenBy(r => r.QueuedAt).ThenBy(r => r.Id).FirstOrDefaultAsync(ct);
            if (run is null)
                return null;
            run.Start(clock.UtcNow);
            await db.SaveChangesAsync(ct);
            return run.Id;
        }
        var now = clock.UtcNow;
        var ids = await db.Database.SqlQuery<int>($"""
            UPDATE public."TB_RUN" SET "STATUS" = 'Running', "STARTED_AT" = COALESCE("STARTED_AT", {now}), "HEARTBEAT_AT" = {now}
            WHERE "ID" = (
                SELECT r."ID" FROM public."TB_RUN" r
                WHERE r."STATUS" = 'Queued'
                ORDER BY (SELECT count(*) FROM public."TB_RUN" x
                          WHERE x."CUSTOMER_ID" = r."CUSTOMER_ID" AND x."STATUS" IN ('Running', 'Cancelling')),
                         r."QUEUED_AT", r."ID"
                FOR UPDATE OF r SKIP LOCKED
                LIMIT 1)
            RETURNING "ID" AS "Value"
            """).ToListAsync(ct);
        return ids.Count == 0 ? null : ids[0];
    }
}
