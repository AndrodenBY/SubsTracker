using DispatchR.Abstractions.Notification;
using SubsTracker.DAL.Models.Subscription;
using SubsTracker.Domain.Enums;

namespace SubsTracker.BLL.Handlers.Notifications;

public record SubscriptionUpdatedNotification(Subscription Subscription, Guid UserId, SubscriptionType OriginalType) : INotification;
