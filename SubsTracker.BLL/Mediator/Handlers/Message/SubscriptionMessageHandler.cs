using DispatchR.Abstractions.Notification;
using SubsTracker.BLL.Helpers.Messages;
using SubsTracker.BLL.Mediator.Signals;
using SubsTracker.Messaging.Interfaces;

namespace SubsTracker.BLL.Mediator.Handlers.Message;

public class SubscriptionMessageHandler(IMessageService messageService) 
    : INotificationHandler<SubscriptionSignals.Canceled>, 
      INotificationHandler<SubscriptionSignals.Renewed>
{
    public async ValueTask Handle(SubscriptionSignals.Canceled signal, CancellationToken cancellationToken)
    {
        var emailEvent = SubscriptionMessageHelper.CreateSubscriptionCanceledEvent(signal.Subscription);
        await messageService.NotifySubscriptionCanceled(emailEvent, cancellationToken);
    }
    
    public async ValueTask Handle(SubscriptionSignals.Renewed signal, CancellationToken cancellationToken)
    {
        var subscriptionRenewedEvent = SubscriptionMessageHelper.CreateSubscriptionRenewedEvent(signal.Subscription);
        await messageService.NotifySubscriptionRenewed(subscriptionRenewedEvent, cancellationToken);
    }
}
