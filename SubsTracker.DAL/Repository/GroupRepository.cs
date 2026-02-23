using Microsoft.EntityFrameworkCore;
using SubsTracker.DAL.Entities;
using SubsTracker.DAL.Interfaces.Repositories;

namespace SubsTracker.DAL.Repository;

public class GroupRepository(SubsDbContext context) : Repository<GroupEntity>(context), IGroupRepository
{
    private readonly DbSet<GroupEntity> _dbSet = context.Set<GroupEntity>();

    public Task<GroupEntity?> GetFullInfoById(Guid id, CancellationToken cancellationToken)
    {
        return _dbSet
            .AsSplitQuery()
            .Include(g => g.SharedSubscriptions)
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }
}
