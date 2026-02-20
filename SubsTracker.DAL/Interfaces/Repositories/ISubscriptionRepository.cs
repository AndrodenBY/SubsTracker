using SubsTracker.DAL.Entities.Subscription;

namespace SubsTracker.DAL.Interfaces.Repositories;

public interface ISubscriptionRepository : IRepository<SubscriptionEntity>
{
    Task<SubscriptionEntity?> GetUserInfoById(Guid id, CancellationToken cancellationToken);
    Task<List<SubscriptionEntity>> GetUpcomingBills(Guid userId, CancellationToken cancellationToken);
}
