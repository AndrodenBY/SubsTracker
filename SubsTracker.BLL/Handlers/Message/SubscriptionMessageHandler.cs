using DispatchR.Abstractions.Notification;
using SubsTracker.BLL.Handlers.Signals;
using SubsTracker.BLL.Handlers.Signals.Subscription;
using SubsTracker.BLL.Helpers.Notifications;
using SubsTracker.Messaging.Interfaces;

namespace SubsTracker.BLL.Handlers.Message;

public class SubscriptionMessageHandler(IMessageService messageService) 
    : INotificationHandler<SubscriptionCanceledSignal>, 
        INotificationHandler<SubscriptionRenewedSignal>
{
    public async ValueTask Handle(SubscriptionCanceledSignal signal, CancellationToken cancellationToken)
    {
        var emailEvent = SubscriptionNotificationHelper.CreateSubscriptionCanceledEvent(signal.Subscription);
        await messageService.NotifySubscriptionCanceled(emailEvent, cancellationToken);
    }
    
    public async ValueTask Handle(SubscriptionRenewedSignal signal, CancellationToken cancellationToken)
    {
        var subscriptionRenewedEvent = SubscriptionNotificationHelper.CreateSubscriptionRenewedEvent(signal.Subscription);
        await messageService.NotifySubscriptionRenewed(subscriptionRenewedEvent, cancellationToken);
    }
}
