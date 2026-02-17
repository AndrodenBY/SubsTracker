using DispatchR.Abstractions.Notification;
using SubsTracker.DAL.Models.Subscription;

namespace SubsTracker.BLL.Handlers.Notifications;

public record SubscriptionRenewedNotification(Subscription Subscription, Guid UserId) : INotification;
