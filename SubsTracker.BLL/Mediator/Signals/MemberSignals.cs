using DispatchR.Abstractions.Notification;
using SubsTracker.Domain.Enums;

namespace SubsTracker.BLL.Mediator.Signals;

public class MemberSignals
{
    public record ChangedRole(Guid MemberId, Guid GroupId, Guid UserId, string GroupName, string UserEmail, MemberRole NewRole) : INotification;
    public record Joined(Guid UserId, Guid GroupId) : INotification;
    public record Left(Guid MemberId, Guid GroupId, Guid UserId, string GroupName, string UserEmail): INotification;
}
