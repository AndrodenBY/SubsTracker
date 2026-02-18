using DispatchR.Abstractions.Notification;

namespace SubsTracker.BLL.Handlers.Signals.Member;

public record MemberLeftSignal(Guid MemberId, Guid GroupId, string GroupName, string UserEmail): INotification;
