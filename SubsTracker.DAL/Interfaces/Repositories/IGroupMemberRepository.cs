using SubsTracker.DAL.Models.User;

namespace SubsTracker.DAL.Interfaces.Repositories;

public interface IGroupMemberRepository : IRepository<GroupMember>
{
    Task<GroupMember?> GetFullInfoById(Guid id, CancellationToken cancellationToken);
}
