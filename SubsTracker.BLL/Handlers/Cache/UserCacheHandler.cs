using DispatchR.Abstractions.Notification;
using SubsTracker.BLL.Handlers.Signals.User;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.DAL.Models.User;

namespace SubsTracker.BLL.Handlers.Cache;

public class UserCacheHandler(ICacheService cacheService) 
    : INotificationHandler<UserCreatedSignal>,
        INotificationHandler<UserUpdatedSignal>,
        INotificationHandler<UserDeletedSignal>
{
    public async ValueTask Handle(UserCreatedSignal notification, CancellationToken cancellationToken) 
        => await InvalidateUserEntry(notification.ExternalId, cancellationToken);
    
    public async ValueTask Handle(UserUpdatedSignal notification, CancellationToken cancellationToken) 
        => await InvalidateUserEntry(notification.ExternalId, cancellationToken);
    
    public async ValueTask Handle(UserDeletedSignal notification, CancellationToken cancellationToken) 
        => await InvalidateUserEntry(notification.ExternalId, cancellationToken);

    private async Task InvalidateUserEntry(string externalId, CancellationToken cancellationToken)
    {
        await cacheService.InvalidateCache<User>(
            externalId, 
            cancellationToken
        );
    }
}
