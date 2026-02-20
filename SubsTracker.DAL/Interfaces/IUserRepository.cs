using SubsTracker.DAL.Entities.User;
using SubsTracker.DAL.Interfaces.Repositories;

namespace SubsTracker.DAL.Interfaces;

public interface IUserRepository : IRepository<UserEntity>
{
    Task<UserEntity?> GetByAuth0Id(string auth0Id, CancellationToken cancellationToken, bool isTracking = true);
}
