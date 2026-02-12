using Microsoft.EntityFrameworkCore;
using SubsTracker.DAL.Interfaces;
using SubsTracker.DAL.Models.User;

namespace SubsTracker.DAL.Repository;

public class UserRepository(SubsDbContext context) : Repository<User>(context), IUserRepository
{
    private readonly DbSet<User> _dbSet = context.Set<User>();
    
    public async Task<User?> GetByAuth0Id(string auth0Id, CancellationToken cancellationToken, bool isTracking = true)
    {
        var query = isTracking 
            ? _dbSet.AsSplitQuery() 
            : _dbSet.AsSplitQuery().AsNoTracking();
        
        return await query 
            .Include(u => u.Subscriptions) 
            .Include(u => u.Groups) 
            .FirstOrDefaultAsync(u => u.Auth0Id == auth0Id, cancellationToken);
    }
}
