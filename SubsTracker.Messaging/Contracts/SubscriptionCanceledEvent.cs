namespace SubsTracker.Messaging.Contracts;

public record SubscriptionCanceledEvent(Guid SubscriptionId, string SubscriptionName, Guid UserId, string Email) : BaseEvent;
