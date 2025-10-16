using Microsoft.EntityFrameworkCore;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.DAL.Models.User;

namespace SubsTracker.DAL.Repository;

public class GroupMemberRepository(SubsDbContext context) : Repository<GroupMember>(context), IGroupMemberRepository
{
    private readonly DbSet<GroupMember> _dbSet = context.Set<GroupMember>();
    
    public Task<GroupMember?> GetFullInfoById(Guid id, CancellationToken cancellationToken)
    {
        return _dbSet
            .Include(g => g.User)
            .Include(g => g.Group)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }
}
