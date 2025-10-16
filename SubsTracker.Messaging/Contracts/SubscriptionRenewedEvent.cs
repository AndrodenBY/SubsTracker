namespace SubsTracker.Messaging.Contracts;

public record SubscriptionRenewedEvent(Guid SubscriptionId, string Name, Guid UserId, DateOnly NewExpirationDate, string Email) : BaseEvent;
