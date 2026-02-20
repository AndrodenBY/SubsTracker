using System.Linq.Expressions;
using SubsTracker.DAL.Entities.User;

namespace SubsTracker.DAL.Interfaces.Repositories;

public interface IGroupMemberRepository : IRepository<GroupMember>
{
    Task<GroupMember?> GetFullInfoById(Guid id, CancellationToken cancellationToken);
    new Task<bool> Delete(GroupMember entityToDelete, CancellationToken cancellationToken);

    Task<GroupMember?> GetByPredicateFullInfo(Expression<Func<GroupMember, bool>> predicate,
        CancellationToken cancellationToken);
}
