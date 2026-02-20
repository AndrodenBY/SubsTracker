using SubsTracker.DAL.Entities;
using SubsTracker.Messaging.Contracts;

namespace SubsTracker.BLL.Helpers.Notifications;

public static class SubscriptionNotificationHelper
{
    public static SubscriptionCanceledEvent CreateSubscriptionCanceledEvent(SubscriptionEntity subscriptionEntity)
    {
        return new SubscriptionCanceledEvent(
            subscriptionEntity.Id,
            subscriptionEntity.Name,
            subscriptionEntity.User!.Id,
            subscriptionEntity.User.Email
        );
    }

    public static SubscriptionRenewedEvent CreateSubscriptionRenewedEvent(SubscriptionEntity subscriptionEntity)
    {
        return new SubscriptionRenewedEvent(
            subscriptionEntity.Id,
            subscriptionEntity.Name,
            subscriptionEntity.User!.Id,
            subscriptionEntity.DueDate,
            subscriptionEntity.User.Email
        );
    }
}
