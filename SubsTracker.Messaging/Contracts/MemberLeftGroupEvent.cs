namespace SubsTracker.Messaging.Contracts;

public record MemberLeftGroupEvent(Guid MemberId, Guid GroupId, string GroupName, string Email) : BaseEvent;
