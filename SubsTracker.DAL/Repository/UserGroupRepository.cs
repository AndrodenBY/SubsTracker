using Microsoft.EntityFrameworkCore;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.DAL.Models.User;

namespace SubsTracker.DAL.Repository;

public class UserGroupRepository(SubsDbContext context) : Repository<UserGroup>(context), IUserGroupRepository
{
    public async Task<UserGroup?> GetById(Guid id, CancellationToken cancellationToken)
    {
        var i = await context.UserGroups
            .Include(g => g.SharedSubscriptions)
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
        return i;
    }
}
