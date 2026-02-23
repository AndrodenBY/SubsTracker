using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SubsTracker.DAL.Interfaces;
using SubsTracker.DAL.Interfaces.Repositories;

namespace SubsTracker.DAL.Repository;

public class Repository<TEntity>(SubsDbContext context) : IRepository<TEntity> where TEntity : class, IBaseEntity
{
    private readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();
    protected readonly SubsDbContext Context = context;

    public Task<List<TEntity>> GetAll(Expression<Func<TEntity, bool>>? expression, CancellationToken cancellationToken)
    {
        var query = _dbSet
            .AsQueryable()
            .AsNoTracking();

        if (expression is not null) query = query.Where(expression);

        return query.ToListAsync(cancellationToken);
    }

    public virtual Task<TEntity?> GetById(Guid id, CancellationToken cancellationToken)
    {
        return _dbSet.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public async Task<TEntity> Create(TEntity entityToCreate, CancellationToken cancellationToken)
    {
        _dbSet.Add(entityToCreate);
        await Context.SaveChangesAsync(cancellationToken);
        return entityToCreate;
    }

    public async Task<TEntity> Update(TEntity entityToUpdate, CancellationToken cancellationToken)
    {
        var existingEntity = Context.Update(entityToUpdate);
        await Context.SaveChangesAsync(cancellationToken);
        
        return existingEntity.Entity;
    }

    public async Task<bool> Delete(TEntity entityToDelete, CancellationToken cancellationToken)
    {
        _dbSet.Remove(entityToDelete);
        return await Context.SaveChangesAsync(cancellationToken) > 0;
    }

    public Task<TEntity?> GetByPredicate(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken)
    {
        return _dbSet.FirstOrDefaultAsync(expression, cancellationToken);
    }
}
