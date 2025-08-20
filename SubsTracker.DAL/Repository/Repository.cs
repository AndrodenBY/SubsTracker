using Microsoft.EntityFrameworkCore;
using SubsTracker.DAL.Models;
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
    
    public async Task<IEnumerable<TModel>?> GetAll(CancellationToken cancellationToken)
    {
        return await _dbSet.ToListAsync();
    }
    
    public async Task<TModel?> GetById(Guid id, CancellationToken cancellationToken)
    {
        return await _dbSet.FindAsync(id);
    }
    
    public async Task<bool> Create(TModel entityToCreate, CancellationToken cancellationToken)
    {
        await _dbSet.AddAsync(entityToCreate, cancellationToken);
        return await _context.SaveChangesAsync(cancellationToken) > 0;
    }
    
    public async Task<bool> Update(TModel entityToUpdate, CancellationToken cancellationToken)
    {
        var existingEntity = await _dbSet.FindAsync(entityToUpdate.Id, cancellationToken);
        
        if (existingEntity is null) return false;
        _context.Update(existingEntity);
        return await _context.SaveChangesAsync(cancellationToken) > 0;
    }
    
    public async Task<bool> Delete(Guid id, CancellationToken cancellationToken)
    {
        var existingEntity = await _dbSet.FindAsync(id, cancellationToken);
        _dbSet.Remove(existingEntity);
        return await _context.SaveChangesAsync(cancellationToken) > 0;
    }
}