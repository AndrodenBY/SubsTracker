using SubsTracker.DAL.Models.Subscription;
using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Interfaces;

namespace SubsTracker.DAL.Interfaces;

public interface ISubscriptionHistoryRepository : IRepository<SubscriptionHistory>
{
    Task<bool> Create(Guid subscriptionId, SubscriptionAction action, decimal? pricePaid, CancellationToken cancellationToken);
    Task UpdateType(SubscriptionType originalType, SubscriptionType updatedType, Guid subscriptionId, decimal? price, CancellationToken cancellationToken);
}
