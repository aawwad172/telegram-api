namespace Telegram.API.Domain.Interfaces.Infrastructure;

public interface ICreate<TEntity>
{
    /// <summary>Create one entity and return the created instance (with keys populated).</summary>
    Task<TEntity?> CreateAsync(TEntity entity, CancellationToken ct = default);
}

public interface IRead<TEntity, in TKey>
{
    /// <summary>Get by primary key; returns null if not found.</summary>
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default);

    /// <summary>List entities (simple pagination). Implementers may ignore skip/take if unsupported.</summary>
    Task<IReadOnlyList<TEntity>> ListAsync(int skip = 0, int take = 100, CancellationToken ct = default);
}

public interface IUpdate<TEntity, in TKey>
{
    /// <summary>Update entity identified by id. Returns true if a row was updated.</summary>
    Task<bool> UpdateAsync(TKey id, TEntity entity, CancellationToken ct = default);
}

public interface IDelete<in TKey>
{
    /// <summary>Delete by id. Returns true if a row was deleted.</summary>
    Task<bool> DeleteAsync(TKey id, CancellationToken ct = default);
}

/// <summary>Full CRUD by composition.</summary>
public interface ICRUD<TEntity, TKey> :
    ICreate<TEntity>,
    IRead<TEntity, TKey>,
    IUpdate<TEntity, TKey>,
    IDelete<TKey>
{ }
