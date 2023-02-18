using System.Linq.Expressions;
using Brobot.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Brobot.Repositories;

public abstract class RepositoryBase<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class
{
    protected readonly BrobotDbContext _context;

    public RepositoryBase(BrobotDbContext context)
    {
        _context = context;
    }

    public async virtual Task Add(TEntity entity)
    {
        await _context.Set<TEntity>().AddAsync(entity);
    }

    public async virtual Task AddRange(IEnumerable<TEntity> entities)
    {
        await _context.Set<TEntity>().AddRangeAsync(entities);
    }

    public async virtual Task<IEnumerable<TEntity>> Find(Expression<Func<TEntity, bool>> expression)
    {
        return await _context.Set<TEntity>().Where(expression).ToListAsync();
    }

    public async virtual Task<IEnumerable<TEntity>> GetAll()
    {
        return await _context.Set<TEntity>().ToListAsync();
    }

    public async virtual Task<TEntity?> GetById(TKey id)
    {
        return await _context.Set<TEntity>().FindAsync(id);
    }

    public virtual void Remove(TEntity entity)
    {
        _context.Set<TEntity>().Remove(entity);
    }

    public virtual void RemoveRange(IEnumerable<TEntity> entities)
    {
        _context.Set<TEntity>().RemoveRange(entities);
    }
}