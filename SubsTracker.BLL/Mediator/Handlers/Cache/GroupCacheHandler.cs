using DispatchR.Abstractions.Notification;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.BLL.Mediator.Signals;
using SubsTracker.BLL.RedisSettings;
using SubsTracker.DAL.Entities;

namespace SubsTracker.BLL.Mediator.Handlers.Cache;

public class GroupCacheHandler(ICacheService cacheService)
    : INotificationHandler<GroupSignals.Created>,
      INotificationHandler<GroupSignals.Updated>,
      INotificationHandler<GroupSignals.Deleted>
{
    public async ValueTask Handle(GroupSignals.Created signal, CancellationToken cancellationToken)
    {
        await cacheService.RemoveData(
            [RedisKeySetter.SetCacheKey(signal.UserId, "user_groups_list")], 
            cancellationToken);
    }
    
    public async ValueTask Handle(GroupSignals.Updated signal, CancellationToken cancellationToken)
        => await InvalidateGroupEntry(signal.GroupId, signal.UserId, cancellationToken);

    public async ValueTask Handle(GroupSignals.Deleted signal, CancellationToken cancellationToken)
        => await InvalidateGroupEntry(signal.GroupId, signal.UserId, cancellationToken);
    
    private async ValueTask InvalidateGroupEntry(Guid groupId, Guid userId, CancellationToken cancellationToken)
    {
        await cacheService.InvalidateCache<GroupEntity>(
            groupId, 
            cancellationToken, 
            RedisKeySetter.SetCacheKey(userId, "user_groups_list"));
    }
}
