using DispatchR.Abstractions.Notification;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.BLL.Mediator.Signals;
using SubsTracker.DAL.Entities;

namespace SubsTracker.BLL.Mediator.Handlers.Cache;

public class MemberCacheHandler(ICacheService cacheService)
    : INotificationHandler<MemberSignals.Joined>,
      INotificationHandler<MemberSignals.Left>,
      INotificationHandler<MemberSignals.ChangedRole>
{
    public async ValueTask Handle(MemberSignals.Joined signal, CancellationToken cancellationToken)
        => await InvalidateRelatedCaches(signal.GroupId, signal.UserId, cancellationToken);

    public async ValueTask Handle(MemberSignals.Left signal, CancellationToken cancellationToken)
        => await InvalidateRelatedCaches(signal.GroupId, signal.UserId, cancellationToken);

    public async ValueTask Handle(MemberSignals.ChangedRole signal, CancellationToken cancellationToken)
        => await InvalidateRelatedCaches(signal.GroupId, signal.UserId, cancellationToken);

    private async Task InvalidateRelatedCaches(Guid groupId, Guid userId, CancellationToken ct)
    {
        await cacheService.InvalidateCache<GroupEntity>(groupId, ct);
        await cacheService.InvalidateCache<UserEntity>(userId, ct);
    }
}
