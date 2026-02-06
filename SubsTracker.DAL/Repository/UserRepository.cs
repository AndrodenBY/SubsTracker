using Microsoft.EntityFrameworkCore;
using SubsTracker.DAL.Interfaces;
using SubsTracker.DAL.Models.User;

namespace SubsTracker.DAL.Repository;

public class UserRepository(SubsDbContext context) : Repository<User>(context), IUserRepository
{
    private readonly DbSet<User> _dbSet = context.Set<User>();
    
    public async Task<User?> GetByAuth0Id(string auth0Id, CancellationToken cancellationToken)
    {
        return await _dbSet.
            AsSplitQuery().
            AsNoTracking().
            Include(s => s.Subscriptions).
            Include(g => g.Groups).
            FirstOrDefaultAsync(g => g.Auth0Id == auth0Id, cancellationToken);
    }
}
