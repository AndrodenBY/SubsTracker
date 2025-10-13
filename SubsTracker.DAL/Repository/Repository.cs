using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SubsTracker.DAL.Interfaces;
using SubsTracker.DAL.Interfaces.Repositories;

namespace SubsTracker.DAL.Repository;

public class Repository<TEntity>(SubsDbContext context) : IRepository<TEntity> where TEntity : class, IBaseModel
{
    private readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();
    protected readonly SubsDbContext Context = context;

    public Task<List<TEntity>> GetAll(
        Expression<Func<TEntity, bool>>? predicate, CancellationToken cancellationToken)
    {
        var query = _dbSet
            .AsQueryable()
            .AsNoTracking();

        if (predicate is not null) query = query.Where(predicate);

        return query.ToListAsync(cancellationToken);
    }

    public virtual Task<TEntity?> GetById(Guid id, CancellationToken cancellationToken)
    {
        return _dbSet.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public async Task<TEntity> Create(TEntity entityToCreate, CancellationToken cancellationToken)
    {
        await _dbSet.AddAsync(entityToCreate, cancellationToken);
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

    public Task<TEntity?> GetByPredicate(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }
}
