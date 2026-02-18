using DispatchR.Abstractions.Notification;
using SubsTracker.Domain.Enums;

namespace SubsTracker.BLL.Handlers.Signals.Subscription;

public record SubscriptionUpdatedSignal(DAL.Models.Subscription.Subscription Subscription, Guid UserId, SubscriptionType OriginalType) : INotification;
