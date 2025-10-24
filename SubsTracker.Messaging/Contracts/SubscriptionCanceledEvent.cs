namespace SubsTracker.Messaging.Contracts;

public record SubscriptionCanceledEvent(Guid Id, string Name, Guid UserId, string Email) : BaseEvent;
