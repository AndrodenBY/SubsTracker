using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SubsTracker.DAL.Interfaces;
using SubsTracker.DAL.Interfaces.Repositories;

namespace SubsTracker.DAL.Repository;

public class Repository<TEntity>(SubsDbContext context) : IRepository<TEntity> where TEntity : class, IBaseModel
{
    private readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();
    
    public async Task<IEnumerable<TEntity>> GetAll(
        Expression<Func<TEntity, bool>>? predicate, CancellationToken cancellationToken)
    {
        return predicate == null
            ? await _dbSet.ToListAsync(cancellationToken)
            : await _dbSet.Where(predicate).ToListAsync(cancellationToken);
    }
    
    public async Task<TEntity?> GetById(Guid id, CancellationToken cancellationToken)
    {
        return await _dbSet.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }
    
    public async Task<TEntity> Create(TEntity entityToCreate, CancellationToken cancellationToken)
    {
        await _dbSet.AddAsync(entityToCreate, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return entityToCreate;
    }
    
    public async Task<TEntity> Update(TEntity entityToUpdate, CancellationToken cancellationToken)
    {
        var existingEntity = context.Update(entityToUpdate);
        await context.SaveChangesAsync(cancellationToken);
        return existingEntity.Entity;
    }
    
    public async Task<bool> Delete(TEntity entityToDelete, CancellationToken cancellationToken)
    {
        _dbSet.Remove(entityToDelete);
        return await context.SaveChangesAsync(cancellationToken) > 0;
    }
    
    public async Task<TEntity?> GetByPredicate(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }
}
