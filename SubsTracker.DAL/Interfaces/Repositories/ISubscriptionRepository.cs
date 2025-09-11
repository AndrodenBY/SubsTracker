using SubsTracker.DAL.Models.Subscription;

namespace SubsTracker.DAL.Interfaces.Repositories;

public interface ISubscriptionRepository : IRepository<Subscription>
{
    Task<IEnumerable<Subscription>> GetUpcomingBills(Guid userId, CancellationToken cancellationToken);
}
