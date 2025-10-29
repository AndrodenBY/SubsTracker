using Microsoft.EntityFrameworkCore;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.DAL.Models.User;

namespace SubsTracker.DAL.Repository;

public class UserGroupRepository(SubsDbContext context) : Repository<UserGroup>(context), IUserGroupRepository
{
    private readonly DbSet<UserGroup> _dbSet = context.Set<UserGroup>();

    public Task<UserGroup?> GetFullInfoById(Guid id, CancellationToken cancellationToken)
    {
        return _dbSet
            .AsSplitQuery()
            .Include(g => g.SharedSubscriptions)
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }
}
