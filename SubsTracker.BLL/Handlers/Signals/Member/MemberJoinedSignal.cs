using DispatchR.Abstractions.Notification;

namespace SubsTracker.BLL.Handlers.Signals.Member;

public record MemberJoinedSignal(Guid UserId, Guid GroupId, string GroupName, string UserEmail) : INotification;
