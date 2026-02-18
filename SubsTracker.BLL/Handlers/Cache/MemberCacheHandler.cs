using DispatchR.Abstractions.Notification;
using SubsTracker.BLL.Handlers.Signals.Member;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.BLL.RedisSettings;
using SubsTracker.DAL.Models.User;

namespace SubsTracker.BLL.Handlers.Cache;

public class MemberCacheHandler(ICacheService cacheService) 
    : INotificationHandler<MemberJoinedSignal>,
        INotificationHandler<MemberLeftSignal>,
        INotificationHandler<MemberChangedRoleSignal>
{
    public async ValueTask Handle(MemberJoinedSignal signal, CancellationToken cancellationToken) 
        => await InvalidateParentCaches(signal.GroupId, cancellationToken);

    public async ValueTask Handle(MemberLeftSignal signal, CancellationToken cancellationToken) 
        => await InvalidateParentCaches(signal.GroupId, cancellationToken);

    public async ValueTask Handle(MemberChangedRoleSignal signal, CancellationToken cancellationToken) 
        => await InvalidateParentCaches(signal.GroupId, cancellationToken);

    private async Task InvalidateParentCaches(Guid groupId, CancellationToken cancellationToken)
    {
        await cacheService.InvalidateCache<UserGroup>(
            groupId, 
            cancellationToken);
    }
}
