using SubsTracker.DAL.Entities;

namespace SubsTracker.DAL.Interfaces.Repositories;

public interface IUserRepository : IRepository<UserEntity>
{
    Task<UserEntity?> GetByIdentityId(string identityId, CancellationToken cancellationToken, bool isTracking = true);
}
