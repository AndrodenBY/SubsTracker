namespace SubsTracker.Messaging.Contracts;

public record SubscriptionCanceledEvent(Guid SubscriptionId, string Name, Guid UserId, string Email) : BaseEvent;
