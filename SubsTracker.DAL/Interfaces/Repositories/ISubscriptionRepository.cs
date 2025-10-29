using SubsTracker.DAL.Models.Subscription;

namespace SubsTracker.DAL.Interfaces.Repositories;

public interface ISubscriptionRepository : IRepository<Subscription>
{
    Task<Subscription?> GetUserInfoById(Guid id, CancellationToken cancellationToken);
    Task<List<Subscription>> GetUpcomingBills(Guid userId, CancellationToken cancellationToken);
}