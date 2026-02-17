using DispatchR.Abstractions.Notification;

namespace SubsTracker.BLL.Handlers.Notifications;

public record SubscriptionDeletedNotification(Guid SubscriptionId, Guid UserId) : INotification;
