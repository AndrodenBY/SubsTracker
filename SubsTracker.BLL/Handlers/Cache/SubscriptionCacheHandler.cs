using DispatchR.Abstractions.Notification;
using SubsTracker.BLL.Handlers.Signals;
using SubsTracker.BLL.Handlers.Signals.Subscription;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.BLL.RedisSettings;
using SubsTracker.DAL.Models.Subscription;

namespace SubsTracker.BLL.Handlers.Cache;

public class SubscriptionCacheHandler(ICacheService cacheService) 
    : INotificationHandler<SubscriptionUpdatedSignal>, 
        INotificationHandler<SubscriptionDeletedSignal>, 
        INotificationHandler<SubscriptionCanceledSignal>, 
        INotificationHandler<SubscriptionRenewedSignal>
{
    public ValueTask Handle(SubscriptionUpdatedSignal signal, CancellationToken cancellationToken)
        => InvalidateSubscriptionEntries(signal.Subscription.Id, signal.UserId, cancellationToken);
    
    public ValueTask Handle(SubscriptionDeletedSignal signal, CancellationToken cancellationToken) 
        => InvalidateSubscriptionEntries(signal.SubscriptionId, signal.UserId, cancellationToken);
    
    public ValueTask Handle(SubscriptionCanceledSignal signal, CancellationToken cancellationToken) 
        => InvalidateSubscriptionEntries(signal.Subscription.Id, signal.UserId, cancellationToken);

    public ValueTask Handle(SubscriptionRenewedSignal signal, CancellationToken cancellationToken)
        => InvalidateSubscriptionEntries(signal.Subscription.Id, signal.UserId, cancellationToken);
    
    private ValueTask InvalidateSubscriptionEntries(Guid subscriptionId, Guid userId, CancellationToken cancellationToken)
    {
        return new ValueTask(cacheService.InvalidateCache<Subscription>(
            subscriptionId, 
            cancellationToken, 
            RedisKeySetter.SetCacheKey(userId, "upcoming_bills")
        ));
    }
}
