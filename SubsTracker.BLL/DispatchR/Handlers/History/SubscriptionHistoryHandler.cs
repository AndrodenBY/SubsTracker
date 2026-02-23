using DispatchR.Abstractions.Notification;
using SubsTracker.BLL.DispatchR.Signals;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.Domain.Enums;

namespace SubsTracker.BLL.DispatchR.Handlers.History;

public class SubscriptionHistoryHandler(ISubscriptionHistoryRepository historyRepository)
    : INotificationHandler<SubscriptionSignals.Updated>, 
      INotificationHandler<SubscriptionSignals.Canceled>,
      INotificationHandler<SubscriptionSignals.Renewed>
{
    public async ValueTask Handle(SubscriptionSignals.Updated notification, CancellationToken cancellationToken)
    {
        await historyRepository.UpdateType(
            notification.OriginalType,
            notification.Subscription.Type,
            notification.Subscription.Id,
            notification.Subscription.Price,
            cancellationToken);
    }
    
    public async ValueTask Handle(SubscriptionSignals.Canceled signal, CancellationToken cancellationToken)
    {
        await historyRepository.Create(
            signal.Subscription.Id, 
            SubscriptionAction.Cancel, 
            null, 
            cancellationToken);
    }
    
    public async ValueTask Handle(SubscriptionSignals.Renewed signal, CancellationToken cancellationToken)
    {
        await historyRepository.Create(
            signal.Subscription.Id, 
            SubscriptionAction.Renew, 
            null, 
            cancellationToken);
    }
}
