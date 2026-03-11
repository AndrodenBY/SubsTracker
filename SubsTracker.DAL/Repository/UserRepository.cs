using Microsoft.EntityFrameworkCore;
using SubsTracker.DAL.Entities;
using SubsTracker.DAL.Interfaces.Repositories;

namespace SubsTracker.DAL.Repository;

public class UserRepository(SubsDbContext context) : Repository<UserEntity>(context), IUserRepository
{
    private readonly DbSet<UserEntity> _dbSet = context.Set<UserEntity>();
    
    public async Task<UserEntity?> GetByIdentityId(string identityId, CancellationToken cancellationToken, bool isTracking = true)
    {
        var query = isTracking 
            ? _dbSet.AsSplitQuery() 
            : _dbSet.AsSplitQuery().AsNoTracking();
        
        return await query 
            .Include(u => u.Subscriptions) 
            .Include(u => u.Groups) 
            .FirstOrDefaultAsync(u => u.IdentityId == identityId, cancellationToken);
    }
}
