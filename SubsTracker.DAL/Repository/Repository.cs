using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SubsTracker.Domain.Exceptions;
using SubsTracker.Domain;

namespace SubsTracker.DAL.Repository;

public class Repository<TModel>(SubsDbContext context) : IRepository<TModel> where TModel : class, IBaseModel
{
    private readonly DbSet<TModel> _dbSet = context.Set<TModel>();
    
    public async Task<IEnumerable<TModel>> GetAll(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }
    
    public async Task<TModel?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstAsync(entity => entity.Id == id, cancellationToken);
    }
    
    public async Task<TModel> Create(TModel entityToCreate, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entityToCreate, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return entityToCreate;
    }
    
    public async Task<TModel?> Update(TModel entityToUpdate, CancellationToken cancellationToken = default)
    {
        var existingEntity = await _dbSet.FirstAsync(entity => entity.Id == entityToUpdate.Id, cancellationToken)
            ?? throw new NotFoundException($"Entity {entityToUpdate.Id} not found");
        
        context.Update(existingEntity);
        await context.SaveChangesAsync(cancellationToken);
        return existingEntity;
    }
    
    public async Task<bool> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var existingEntity = await _dbSet.FirstAsync(entity => entity.Id == id, cancellationToken);
        _dbSet.Remove(existingEntity);
        return await context.SaveChangesAsync(cancellationToken) > 0;
    }
    
    public async Task<TModel?> FindByCondition(Expression<Func<TModel, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }
}