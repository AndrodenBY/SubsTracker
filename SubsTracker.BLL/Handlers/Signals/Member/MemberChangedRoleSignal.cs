using DispatchR.Abstractions.Notification;
using SubsTracker.Domain.Enums;

namespace SubsTracker.BLL.Handlers.Signals.Member;

public record MemberChangedRoleSignal(Guid MemberId, Guid GroupId, string GroupName, string UserEmail, MemberRole ChangedRole) : INotification;
