using System.Linq.Expressions;
using Brobot.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Brobot.Repositories;

public abstract class RepositoryBase<TEntity, TKey>(BrobotDbContext context) : IRepository<TEntity, TKey>
    where TEntity : class
{
    protected readonly BrobotDbContext Context = context;

    public virtual async Task Add(TEntity entity)
    {
        await Context.Set<TEntity>().AddAsync(entity);
    }

    public virtual async Task AddRange(IEnumerable<TEntity> entities)
    {
        await Context.Set<TEntity>().AddRangeAsync(entities);
    }

    public virtual async Task<IEnumerable<TEntity>> Find(Expression<Func<TEntity, bool>> expression)
    {
        return await Context.Set<TEntity>().Where(expression).ToListAsync();
    }

    public virtual async Task<IEnumerable<TEntity>> GetAll()
    {
        return await Context.Set<TEntity>().ToListAsync();
    }

    public virtual async Task<TEntity?> GetById(TKey id)
    {
        return await Context.Set<TEntity>().FindAsync(id);
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