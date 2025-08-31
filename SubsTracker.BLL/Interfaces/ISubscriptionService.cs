using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.DAL.Models.Subscription;
using SubsTracker.Domain.Interfaces;

namespace SubsTracker.BLL.Interfaces;

public interface ISubscriptionService : IService<Subscription, SubscriptionDto, CreateSubscriptionDto, UpdateSubscriptionDto>
{
    Task<SubscriptionDto> Create(Guid userId, CreateSubscriptionDto createDto, CancellationToken cancellationToken);
    new Task<SubscriptionDto> Update(Guid userId, UpdateSubscriptionDto updateDto, CancellationToken cancellationToken);
    new Task<bool> Delete(Guid id, CancellationToken cancellationToken);
    Task<SubscriptionDto> RenewSubscription(Guid subscriptionId, int monthsToRenew, CancellationToken cancellationToken);
    Task<IEnumerable<SubscriptionDto>> GetUpcomingBills(Guid userId, CancellationToken cancellationToken);
}