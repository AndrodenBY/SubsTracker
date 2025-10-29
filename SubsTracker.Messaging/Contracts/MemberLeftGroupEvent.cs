namespace SubsTracker.Messaging.Contracts;

public record MemberLeftGroupEvent(Guid Id, Guid GroupId, string GroupName, string Email) : BaseEvent;
