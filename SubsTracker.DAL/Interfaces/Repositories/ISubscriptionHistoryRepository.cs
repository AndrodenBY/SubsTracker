using SubsTracker.DAL.Entities;
using SubsTracker.Domain.Enums;

namespace SubsTracker.DAL.Interfaces.Repositories;

public interface ISubscriptionHistoryRepository : IRepository<SubscriptionHistory>
{
    Task<bool> Create(Guid subscriptionId, SubscriptionAction action, decimal? pricePaid,
        CancellationToken cancellationToken);

    Task UpdateType(SubscriptionType originalType, SubscriptionType updatedType, Guid subscriptionId, decimal? price,
        CancellationToken cancellationToken);
}
