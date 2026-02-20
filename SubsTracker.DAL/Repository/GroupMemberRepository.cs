using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SubsTracker.DAL.Entities.User;
using SubsTracker.DAL.Interfaces.Repositories;

namespace SubsTracker.DAL.Repository;

public class GroupMemberRepository(SubsDbContext context) : Repository<GroupMember>(context), IGroupMemberRepository
{
    private readonly DbSet<GroupMember> _dbSet = context.Set<GroupMember>();

    public Task<GroupMember?> GetFullInfoById(Guid id, CancellationToken cancellationToken)
    {
        return _dbSet
            .AsSplitQuery()
            .Include(g => g.UserEntity)
            .Include(g => g.Group)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public new async Task<bool> Delete(GroupMember entityToDelete, CancellationToken cancellationToken)
    {
        _dbSet.Remove(entityToDelete);
        return await Context.SaveChangesAsync(cancellationToken) > 0;
    }

    public Task<GroupMember?> GetByPredicateFullInfo(Expression<Func<GroupMember, bool>> predicate,
        CancellationToken cancellationToken)
    {
        return _dbSet
            .Include(m => m.UserEntity)
            .Include(m => m.Group)
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }
}
