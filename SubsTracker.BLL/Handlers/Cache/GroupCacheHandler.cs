using DispatchR.Abstractions.Notification;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.Handlers.Signals;
using SubsTracker.BLL.Handlers.Signals.Group;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.BLL.RedisSettings;
using SubsTracker.DAL.Models.User;

namespace SubsTracker.BLL.Handlers.Cache;

public class GroupCacheHandler(ICacheService cacheService) 
    : INotificationHandler<GroupUpdatedSignal>,
        INotificationHandler<GroupDeletedSignal>,
        INotificationHandler<GroupCreatedSignal>
{
    public async ValueTask Handle(GroupUpdatedSignal signal, CancellationToken cancellationToken)
    
        => await InvalidateGroupEntry(signal.GroupId, signal.UserId, cancellationToken);

    public async ValueTask Handle(GroupDeletedSignal signal, CancellationToken cancellationToken) 
        => await InvalidateGroupEntry(signal.GroupId, signal.UserId, cancellationToken);
    
    public async ValueTask Handle(GroupCreatedSignal signal, CancellationToken cancellationToken)
    {
        await cacheService.RemoveData(
            [RedisKeySetter.SetCacheKey(signal.UserId, "user_groups_list")], 
            cancellationToken);
    }
    
    private async ValueTask InvalidateGroupEntry(Guid groupId, Guid userId, CancellationToken cancellationToken)
    {
        await cacheService.InvalidateCache<UserGroup>(
            groupId, 
            cancellationToken, 
            RedisKeySetter.SetCacheKey(userId, "user_groups_list"));
    }
}
