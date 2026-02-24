using DispatchR.Abstractions.Notification;
using SubsTracker.BLL.DispatchR.Signals;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.DAL.Entities;

namespace SubsTracker.BLL.DispatchR.Handlers.Cache;

public class UserCacheHandler(ICacheService cacheService) 
    : INotificationHandler<UserSignals.Created>,
      INotificationHandler<UserSignals.Updated>,
      INotificationHandler<UserSignals.Deleted>
{
    public async ValueTask Handle(UserSignals.Created signal, CancellationToken cancellationToken) 
        => await InvalidateUserEntry(signal.Auth0Id, cancellationToken);
    
    public async ValueTask Handle(UserSignals.Updated signal, CancellationToken cancellationToken) 
        => await InvalidateUserEntry(signal.Auth0Id, cancellationToken);
    
    public async ValueTask Handle(UserSignals.Deleted signal, CancellationToken cancellationToken) 
        => await InvalidateUserEntry(signal.Auth0Id, cancellationToken);

    private async Task InvalidateUserEntry(string identityId, CancellationToken cancellationToken)
    {
        await cacheService.InvalidateCache<UserEntity>(
            identityId, 
            cancellationToken
        );
    }
}
