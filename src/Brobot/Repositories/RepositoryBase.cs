using System.Linq.Expressions;
using Brobot.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Brobot.Repositories;

public abstract class RepositoryBase<TEntity, TKey>(BrobotDbContext context) : IRepository<TEntity, TKey>
    where TEntity : class
{
    protected readonly BrobotDbContext Context = context;

    public virtual async Task Add(TEntity entity, CancellationToken cancellationToken = default)
    {
        await Context.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    public virtual async Task AddRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await Context.Set<TEntity>().AddRangeAsync(entities, cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> Find(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default)
    {
        return await Context.Set<TEntity>().Where(expression).ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAll()
    {
        return await Context.Set<TEntity>().ToListAsync();
    }

    public virtual async Task<TEntity?> GetById(TKey id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<TEntity>().FindAsync(keyValues: [id], cancellationToken: cancellationToken);
    }

    public virtual async Task<TEntity?> GetByIdNoTracking(TKey id)
    {
        var entity = await Context.Set<TEntity>().FindAsync(id);
        if (entity != null)
        {
            Context.Entry(entity).State = EntityState.Detached;
        }

        return entity;
    }

    public virtual void Remove(TEntity entity)
    {
        Context.Set<TEntity>().Remove(entity);
    }

    public virtual void RemoveRange(IEnumerable<TEntity> entities)
    {
        Context.Set<TEntity>().RemoveRange(entities);
    }

    public EntityEntry<TEntity> Entry(TEntity entity)
    {
        return Context.Entry(entity);
    }
}