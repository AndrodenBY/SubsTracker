using SubsTracker.DAL.Models.User;

namespace SubsTracker.DAL.Interfaces.Repositories;

public interface IUserGroupRepository : IRepository<UserGroup>
{
    Task<UserGroup?> GetFullInfoById(Guid id, CancellationToken cancellationToken);
}