using Microsoft.EntityFrameworkCore;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Exceptions;
using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Infra.Database.Mappings;
using Npgsql;

namespace Mignow.Atc.Infra.Database.Context;

/// <summary>IDataStore + IUnitOfWork sobre o EF. Violação de unicidade (23505) vira ConflictException (→ 409).</summary>
public sealed class EfDataStore(AtcDbContext db) : IDataStore, IUnitOfWork
{
    public IQueryable<T> Query<T>() where T : class => db.Set<T>();

    public IQueryable<T> Read<T>() where T : class => db.Set<T>().AsNoTracking();

    public void Add<T>(T entity) where T : class => db.Set<T>().Add(entity);

    public void Remove<T>(T entity) where T : class => db.Set<T>().Remove(entity);

    public Task<List<T>> ListAsync<T>(IQueryable<T> query, CancellationToken ct) => query.ToListAsync(ct);

    public Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> query, CancellationToken ct) => query.FirstOrDefaultAsync(ct);

    public Task<int> CountAsync<T>(IQueryable<T> query, CancellationToken ct) => query.CountAsync(ct);

    public Task<bool> AnyAsync<T>(IQueryable<T> query, CancellationToken ct) => query.AnyAsync(ct);

    public void DiscardChanges() => db.ChangeTracker.Clear();

    public IQueryable<T> Including<T>(IQueryable<T> query, string navigationPath) where T : class => query.Include(navigationPath);

    public async Task<int> SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            return await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pg)
        {
            var code = pg.ConstraintName == RunMapping.ActiveRunIndex ? "Run.EnvironmentBusy" : "Data.Duplicate";
            throw new ConflictException(code, pg.MessageText, ex);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation } pg)
        {
            // Registro ainda referenciado (as exclusões conferem antes; isto cobre a corrida).
            throw new ConflictException("Data.InUse", pg.MessageText, ex);
        }
    }
}
