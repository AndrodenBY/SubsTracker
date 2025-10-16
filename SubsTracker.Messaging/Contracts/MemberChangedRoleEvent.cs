using SubsTracker.Domain.Enums;

namespace SubsTracker.Messaging.Contracts;

public record MemberChangedRoleEvent(Guid MemberId, Guid GroupId, MemberRole Role, string GroupName, string Email) : BaseEvent;
