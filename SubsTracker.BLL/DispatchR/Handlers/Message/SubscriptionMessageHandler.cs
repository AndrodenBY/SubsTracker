using DispatchR.Abstractions.Notification;
using SubsTracker.BLL.DispatchR.Signals;
using SubsTracker.BLL.Helpers.Notifications;
using SubsTracker.Messaging.Interfaces;

namespace SubsTracker.BLL.DispatchR.Handlers.Message;

public class SubscriptionMessageHandler(IMessageService messageService) 
    : INotificationHandler<SubscriptionSignals.Canceled>, 
      INotificationHandler<SubscriptionSignals.Renewed>
{
    public async ValueTask Handle(SubscriptionSignals.Canceled signal, CancellationToken cancellationToken)
    {
        var emailEvent = SubscriptionNotificationHelper.CreateSubscriptionCanceledEvent(signal.Subscription);
        await messageService.NotifySubscriptionCanceled(emailEvent, cancellationToken);
    }
    
    public async ValueTask Handle(SubscriptionSignals.Renewed signal, CancellationToken cancellationToken)
    {
        var subscriptionRenewedEvent = SubscriptionNotificationHelper.CreateSubscriptionRenewedEvent(signal.Subscription);
        await messageService.NotifySubscriptionRenewed(subscriptionRenewedEvent, cancellationToken);
    }
}
