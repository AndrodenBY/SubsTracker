using SubsTracker.Messaging.Enums;

namespace SubsTracker.Messaging.Contracts;

public record MemberChangedRoleEvent(Guid Id, Guid GroupId, MemberRole Role, string GroupName, string Email)
    : BaseEvent;
