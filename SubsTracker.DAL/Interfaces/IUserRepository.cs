using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.DAL.Models.User;

namespace SubsTracker.DAL.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByAuth0Id(string auth0Id, CancellationToken cancellationToken, bool isTracking = true);
}
