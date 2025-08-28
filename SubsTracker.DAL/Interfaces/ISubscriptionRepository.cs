using SubsTracker.DAL.Models.Subscription;
using SubsTracker.Domain;
using SubsTracker.Domain.Interfaces;

namespace SubsTracker.DAL.Interfaces;

public interface ISubscriptionRepository : IRepository<Subscription>
{
    Task<IEnumerable<Subscription>> GetUpcomingBills(Guid userId, CancellationToken cancellationToken);
}