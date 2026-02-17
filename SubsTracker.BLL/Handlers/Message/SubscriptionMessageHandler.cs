using DispatchR.Abstractions.Notification;

using SubsTracker.BLL.Handlers.Notifications;
using SubsTracker.BLL.Helpers.Notifications;
using SubsTracker.Messaging.Interfaces;

namespace SubsTracker.BLL.Handlers.Message;

public class SubscriptionMessageHandler(IMessageService messageService) 
    : INotificationHandler<SubscriptionCanceledNotification>, 
        INotificationHandler<SubscriptionRenewedNotification>
{
    public async ValueTask Handle(SubscriptionCanceledNotification notification, CancellationToken cancellationToken)
    {
        var emailEvent = SubscriptionNotificationHelper.CreateSubscriptionCanceledEvent(notification.Subscription);
        await messageService.NotifySubscriptionCanceled(emailEvent, cancellationToken);
    }
    
    public async ValueTask Handle(SubscriptionRenewedNotification notification, CancellationToken cancellationToken)
    {
        var subscriptionRenewedEvent = SubscriptionNotificationHelper.CreateSubscriptionRenewedEvent(notification.Subscription);
        await messageService.NotifySubscriptionRenewed(subscriptionRenewedEvent, cancellationToken);
    }
}
