using System.Linq.Expressions;
using SubsTracker.DAL.Entities;

namespace SubsTracker.DAL.Interfaces.Repositories;

public interface IMemberRepository : IRepository<MemberEntity>
{
    Task<MemberEntity?> GetFullInfoById(Guid id, CancellationToken cancellationToken);
    new Task<bool> Delete(MemberEntity entityToDelete, CancellationToken cancellationToken);

    Task<MemberEntity?> GetByPredicateFullInfo(Expression<Func<MemberEntity, bool>> expression, CancellationToken cancellationToken);
}
