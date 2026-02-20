using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SubsTracker.DAL.Entities;
using SubsTracker.DAL.Interfaces.Repositories;

namespace SubsTracker.DAL.Repository;

public class MemberRepository(SubsDbContext context) : Repository<MemberEntity>(context), IMemberRepository
{
    private readonly DbSet<MemberEntity> _dbSet = context.Set<MemberEntity>();

    public Task<MemberEntity?> GetFullInfoById(Guid id, CancellationToken cancellationToken)
    {
        return _dbSet
            .AsSplitQuery()
            .Include(g => g.UserEntity)
            .Include(g => g.GroupEntity)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public new async Task<bool> Delete(MemberEntity entityToDelete, CancellationToken cancellationToken)
    {
        _dbSet.Remove(entityToDelete);
        return await Context.SaveChangesAsync(cancellationToken) > 0;
    }

    public Task<MemberEntity?> GetByPredicateFullInfo(Expression<Func<MemberEntity, bool>> predicate,
        CancellationToken cancellationToken)
    {
        return _dbSet
            .Include(m => m.UserEntity)
            .Include(m => m.GroupEntity)
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }
}
