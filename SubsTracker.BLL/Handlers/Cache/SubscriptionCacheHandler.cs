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
    public ValueTask Handle(SubscriptionUpdatedSignal notification, CancellationToken cancellationToken)
        => InvalidateCacheEntries(notification.Subscription.Id, notification.UserId, cancellationToken);
    
    public ValueTask Handle(SubscriptionDeletedSignal notification, CancellationToken cancellationToken) 
        => InvalidateCacheEntries(notification.SubscriptionId, notification.UserId, cancellationToken);
    
    public ValueTask Handle(SubscriptionCanceledSignal signal, CancellationToken cancellationToken) 
        => InvalidateCacheEntries(signal.Subscription.Id, signal.UserId, cancellationToken);

    public ValueTask Handle(SubscriptionRenewedSignal notification, CancellationToken cancellationToken)
        => InvalidateCacheEntries(notification.Subscription.Id, notification.UserId, cancellationToken);
    
    private ValueTask InvalidateCacheEntries(Guid subscriptionId, Guid userId, CancellationToken cancellationToken)
    {
        return new ValueTask(cacheService.InvalidateCache<Subscription>(
            subscriptionId, 
            cancellationToken, 
            RedisKeySetter.SetCacheKey(userId, "upcoming_bills")
        ));
    }
}
