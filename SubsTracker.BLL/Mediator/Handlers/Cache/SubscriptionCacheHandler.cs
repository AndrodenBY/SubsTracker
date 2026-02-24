using DispatchR.Abstractions.Notification;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.BLL.Mediator.Signals;
using SubsTracker.BLL.RedisSettings;
using SubsTracker.DAL.Entities;

namespace SubsTracker.BLL.Mediator.Handlers.Cache;

public class SubscriptionCacheHandler(ICacheService cacheService) 
    : INotificationHandler<SubscriptionSignals.Updated>, 
      INotificationHandler<SubscriptionSignals.Deleted>, 
      INotificationHandler<SubscriptionSignals.Canceled>, 
      INotificationHandler<SubscriptionSignals.Renewed>
{
    public ValueTask Handle(SubscriptionSignals.Updated signal, CancellationToken cancellationToken)
        => InvalidateCacheEntries(signal.Subscription.Id, signal.UserId, cancellationToken);
    
    public ValueTask Handle(SubscriptionSignals.Deleted signal, CancellationToken cancellationToken) 
        => InvalidateCacheEntries(signal.SubscriptionId, signal.UserId, cancellationToken);
    
    public ValueTask Handle(SubscriptionSignals.Canceled signal, CancellationToken cancellationToken) 
        => InvalidateCacheEntries(signal.Subscription.Id, signal.UserId, cancellationToken);

    public ValueTask Handle(SubscriptionSignals.Renewed signal, CancellationToken cancellationToken)
        => InvalidateCacheEntries(signal.Subscription.Id, signal.UserId, cancellationToken);
    
    private ValueTask InvalidateCacheEntries(Guid subscriptionId, Guid userId, CancellationToken cancellationToken)
    {
        return new ValueTask(cacheService.InvalidateCache<SubscriptionEntity>(
            subscriptionId, 
            cancellationToken, 
            RedisKeySetter.SetCacheKey(userId, "upcoming_bills")
        ));
    }
}
