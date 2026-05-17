using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Brobot.Repositories;

public interface IRepository<TEntity, in TKey> where TEntity : class
{
    Task<TEntity?> GetById(TKey id, CancellationToken cancellationToken = default);
    Task<TEntity?> GetByIdNoTracking(TKey id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> GetAll(CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> Find(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default);
    Task Add(TEntity entity, CancellationToken cancellationToken = default);
    Task AddRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    void Remove(TEntity entity);
    void RemoveRange(IEnumerable<TEntity> entities);
    EntityEntry<TEntity> Entry(TEntity entity);
} 