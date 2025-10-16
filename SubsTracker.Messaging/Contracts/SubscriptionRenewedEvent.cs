namespace SubsTracker.Messaging.Contracts;

public record SubscriptionRenewedEvent(Guid SubscriptionId, Guid UserId, DateOnly NewExpirationDate, string Email) : BaseEvent;
