using Mignow.Atc.Application.Ports.Persistence;
using Mignow.Atc.Domain.Common;
using Mignow.Atc.Domain.Exemption;

namespace Mignow.Atc.Tests.Exemption;

/// <summary>IDataStore em memória (sem EF): atribui Ids como o banco e liga os pedidos ao lote como o fixup do EF.</summary>
internal sealed class FakeDataStore : IDataStore
{
    private readonly Dictionary<Type, List<object>> _tables = [];
    private int _next = 1;

    public IQueryable<T> Query<T>() where T : class => Table<T>().OfType<T>().AsQueryable();

    public IQueryable<T> Read<T>() where T : class => Query<T>();

    public void Add<T>(T entity) where T : class
    {
        Register(entity);
        if (entity is ExemptionBatch batch)
            foreach (var r in batch.Requests)
            {
                typeof(ExemptionRequest).GetProperty(nameof(ExemptionRequest.BatchId))!.SetValue(r, batch.Id);
                Register(r);
            }
    }

    public void Remove<T>(T entity) where T : class => Table<T>().Remove(entity);

    public Task<List<T>> ListAsync<T>(IQueryable<T> query, CancellationToken ct) => Task.FromResult(query.ToList());

    public Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> query, CancellationToken ct) => Task.FromResult(query.FirstOrDefault());

    public Task<int> CountAsync<T>(IQueryable<T> query, CancellationToken ct) => Task.FromResult(query.Count());

    public Task<bool> AnyAsync<T>(IQueryable<T> query, CancellationToken ct) => Task.FromResult(query.Any());

    public void DiscardChanges()
    {
    }

    public IQueryable<T> Including<T>(IQueryable<T> query, string navigationPath) where T : class => query;

    public List<object> Table<T>() => Table(typeof(T));

    private List<object> Table(Type t) => _tables.TryGetValue(t, out var l) ? l : _tables[t] = [];

    private void Register(object entity)
    {
        var id = typeof(EntityBase).GetProperty(nameof(EntityBase.Id))!;
        if (id.GetValue(entity) is 0)
            id.SetValue(entity, _next++);
        Table(entity.GetType()).Add(entity);
    }

    /// <summary>Testes: entidade criada fora dos handlers.</summary>
    public T Seed<T>(T entity) where T : class
    {
        Add(entity);
        return entity;
    }
}
