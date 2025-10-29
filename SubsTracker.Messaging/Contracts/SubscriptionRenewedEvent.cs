namespace SubsTracker.Messaging.Contracts;

public record SubscriptionRenewedEvent(Guid Id, string Name, Guid UserId, DateOnly NewExpirationDate, string Email)
    : BaseEvent;
