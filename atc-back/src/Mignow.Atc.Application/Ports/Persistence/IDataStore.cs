namespace Mignow.Atc.Application.Ports.Persistence;

/// <summary>
/// Acesso a dados para os handlers sem depender do EF (a Application não referencia EF; ver CLAUDE.md da Application).
/// <see cref="Query{T}"/> é rastreado (comandos); <see cref="Read{T}"/> é sem rastreio (consultas). A materialização
/// assíncrona passa pelos métodos abaixo, implementados no Infra.Database. O commit é do TransactionDecorator.
/// </summary>
public interface IDataStore
{
    IQueryable<T> Query<T>() where T : class;

    IQueryable<T> Read<T>() where T : class;

    void Add<T>(T entity) where T : class;

    void Remove<T>(T entity) where T : class;

    Task<List<T>> ListAsync<T>(IQueryable<T> query, CancellationToken ct);

    Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> query, CancellationToken ct);

    Task<int> CountAsync<T>(IQueryable<T> query, CancellationToken ct);

    Task<bool> AnyAsync<T>(IQueryable<T> query, CancellationToken ct);

    /// <summary>Descarta as alterações pendentes (depois de um erro ao salvar, para registrar a falha sem repeti-lo).</summary>
    void DiscardChanges();

    /// <summary>Carrega uma navegação de coleção (Include) sem expor o EF.</summary>
    IQueryable<T> Including<T>(IQueryable<T> query, string navigationPath) where T : class;
}
