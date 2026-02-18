using DispatchR.Abstractions.Notification;

namespace SubsTracker.BLL.Handlers.Signals.Subscription;

public record SubscriptionCreatedSignal(DAL.Models.Subscription.Subscription Subscription, Guid UserId) : INotification;
