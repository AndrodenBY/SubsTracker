using SubsTracker.DAL.Models.Subscription;
using SubsTracker.Messaging.Contracts;

namespace SubsTracker.BLL.Helpers.Notifications;

public static class SubscriptionNotificationHelper
{
    public static SubscriptionCanceledEvent CreateSubscriptionCanceledEvent(Subscription subscription)
    {
        return new SubscriptionCanceledEvent(
            subscription.Id,
            subscription.Name,
            subscription.User!.Id,
            subscription.User.Email
        );
    }

    public static SubscriptionRenewedEvent CreateSubscriptionRenewedEvent(Subscription subscription)
    {
        return new SubscriptionRenewedEvent(
            subscription.Id,
            subscription.Name,
            subscription.User!.Id,
            subscription.DueDate,
            subscription.User.Email
        );
    }
}