using DispatchR.Abstractions.Notification;
using SubsTracker.DAL.Entities;
using SubsTracker.Domain.Enums;

namespace SubsTracker.BLL.DispatchR.Signals;

public static class SubscriptionSignals
{
    public record Created(SubscriptionEntity Subscription, Guid UserId) : INotification;
    public record Updated(SubscriptionEntity Subscription, SubscriptionType OriginalType, Guid UserId) : INotification;
    public record Canceled(SubscriptionEntity Subscription, Guid UserId) : INotification;
    public record Deleted(Guid SubscriptionId, Guid UserId) : INotification;
    public record Renewed(SubscriptionEntity Subscription, Guid UserId) : INotification;
}
