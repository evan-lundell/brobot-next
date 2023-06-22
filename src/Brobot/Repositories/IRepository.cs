using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Brobot.Repositories;

public interface IRepository<TEntity, in TKey> where TEntity : class
{
    Task<TEntity?> GetById(TKey id);
    Task<TEntity?> GetByIdNoTracking(TKey id);
    Task<IEnumerable<TEntity>> GetAll();
    Task<IEnumerable<TEntity>> Find(Expression<Func<TEntity, bool>> expression);
    Task Add(TEntity entity);
    Task AddRange(IEnumerable<TEntity> entities);
    void Remove(TEntity entity);
    void RemoveRange(IEnumerable<TEntity> entities);
    EntityEntry<TEntity> Entry(TEntity entity);
}