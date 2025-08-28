using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.DAL.Models.Subscription;
using SubsTracker.Domain.Interfaces;

namespace SubsTracker.BLL.Interfaces;

public interface ISubscriptionService : IService<Subscription, SubscriptionDto, CreateSubscriptionDto, UpdateSubscriptionDto>
{
    Task<SubscriptionDto> RenewSubscription(Guid subscriptionId, CancellationToken cancellationToken);
    Task<IEnumerable<SubscriptionDto>> GetUpcomingBills(Guid userId, CancellationToken cancellationToken);
}