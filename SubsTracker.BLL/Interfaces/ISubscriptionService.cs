using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.DAL.Models.Subscription;
using SubsTracker.Domain.Filter;
using SubsTracker.Domain.Interfaces;

namespace SubsTracker.BLL.Interfaces;

public interface ISubscriptionService : IService<Subscription, SubscriptionDto, CreateSubscriptionDto, UpdateSubscriptionDto, SubscriptionFilter>
{
    Task<IEnumerable<SubscriptionDto>> GetAll(SubscriptionFilter? filter, CancellationToken cancellationToken);
    Task<SubscriptionDto> RenewSubscription(Guid subscriptionId, int monthsToRenew, CancellationToken cancellationToken);
    Task<IEnumerable<SubscriptionDto>> GetUpcomingBills(Guid userId, CancellationToken cancellationToken);
}