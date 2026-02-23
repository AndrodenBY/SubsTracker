using SubsTracker.DAL.Entities;

namespace SubsTracker.DAL.Interfaces.Repositories;

public interface IUserRepository : IRepository<UserEntity>
{
    Task<UserEntity?> GetByAuth0Id(string auth0Id, CancellationToken cancellationToken, bool isTracking = true);
}
