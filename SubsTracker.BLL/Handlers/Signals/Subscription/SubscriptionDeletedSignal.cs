using DispatchR.Abstractions.Notification;

namespace SubsTracker.BLL.Handlers.Signals.Subscription;

public record SubscriptionDeletedSignal(Guid SubscriptionId, Guid UserId) : INotification;
