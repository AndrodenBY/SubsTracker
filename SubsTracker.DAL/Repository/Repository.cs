using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SubsTracker.Domain;

namespace SubsTracker.DAL.Repository;

public class Repository<TModel> : IRepository<TModel> where TModel : class, IBaseModel
{
    private readonly SubsDbContext _context;
    private readonly DbSet<TModel> _dbSet;

    public Repository(SubsDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TModel>();
    }
    
    public async Task<IEnumerable<TModel>> GetAll(CancellationToken cancellationToken)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }
    
    public async Task<TModel?> GetById(Guid id, CancellationToken cancellationToken)
    {
        return await _dbSet.FindAsync(id, cancellationToken);
    }
    
    public async Task<TModel> Create(TModel entityToCreate, CancellationToken cancellationToken)
    {
        await _dbSet.AddAsync(entityToCreate, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entityToCreate;
    }
    
    public async Task<TModel?> Update(TModel entityToUpdate, CancellationToken cancellationToken)
    {
        var existingEntity = await _dbSet.FindAsync(entityToUpdate.Id, cancellationToken)
            ?? throw new NullReferenceException($"Entity {entityToUpdate.Id} not found");
        
        _context.Update(existingEntity);
        await _context.SaveChangesAsync(cancellationToken);
        return existingEntity;
    }
    
    public async Task<bool> Delete(Guid id, CancellationToken cancellationToken)
    {
        var existingEntity = await _dbSet.FindAsync(id, cancellationToken);
        _dbSet.Remove(existingEntity);
        return await _context.SaveChangesAsync(cancellationToken) > 0;
    }
    
    public async Task<TModel?> FindByCondition(Expression<Func<TModel, bool>> predicate, CancellationToken cancellationToken)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }
}