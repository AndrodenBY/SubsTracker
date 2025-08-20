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
    
    public async Task<IEnumerable<TModel>?> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }
    public async Task<TModel?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }
    
    public async Task<bool> CreateAsync(TModel entityToCreate)
    {
        await _dbSet.AddAsync(entityToCreate);
        return await _context.SaveChangesAsync() > 0;
    }
    
    public async Task<bool> UpdateAsync(TModel entityToUpdate)
    {
        var existingEntity = await _dbSet.FindAsync(entityToUpdate.Id);
        
        if (existingEntity is null) return false;
        _context.Update(existingEntity);
        return await _context.SaveChangesAsync() > 0;
    }
    
    public async Task<bool> DeleteAsync(Guid id)
    {
        var existingEntity = await _dbSet.FindAsync(id);
        
        if (existingEntity is null) return false;
        _dbSet.Remove(existingEntity);
        return await _context.SaveChangesAsync() > 0;
    }
}