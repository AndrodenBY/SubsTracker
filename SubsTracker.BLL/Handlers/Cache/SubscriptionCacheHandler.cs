using DispatchR.Abstractions.Notification;
using SubsTracker.BLL.Handlers.Notifications;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.BLL.RedisSettings;
using SubsTracker.DAL.Models.Subscription;

namespace SubsTracker.BLL.Handlers.Cache;

public class SubscriptionCacheHandler(ICacheService cacheService) 
    : INotificationHandler<SubscriptionUpdatedNotification>, 
        INotificationHandler<SubscriptionDeletedNotification>, 
        INotificationHandler<SubscriptionCanceledNotification>, 
        INotificationHandler<SubscriptionRenewedNotification>
{
    public ValueTask Handle(SubscriptionUpdatedNotification notification, CancellationToken cancellationToken)
        => InvalidateCacheEntries(notification.Subscription.Id, notification.UserId, cancellationToken);
    
    public ValueTask Handle(SubscriptionDeletedNotification notification, CancellationToken cancellationToken) 
        => InvalidateCacheEntries(notification.SubscriptionId, notification.UserId, cancellationToken);
    
    public ValueTask Handle(SubscriptionCanceledNotification notification, CancellationToken cancellationToken) 
        => InvalidateCacheEntries(notification.Subscription.Id, notification.UserId, cancellationToken);

    public ValueTask Handle(SubscriptionRenewedNotification notification, CancellationToken cancellationToken)
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
