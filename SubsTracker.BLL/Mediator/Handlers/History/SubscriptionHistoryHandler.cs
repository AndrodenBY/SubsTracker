using DispatchR.Abstractions.Notification;
using SubsTracker.BLL.Mediator.Signals;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.Domain.Enums;

namespace SubsTracker.BLL.Mediator.Handlers.History;

public class SubscriptionHistoryHandler(ISubscriptionHistoryRepository historyRepository)
    : INotificationHandler<SubscriptionSignals.Updated>, 
      INotificationHandler<SubscriptionSignals.Canceled>,
      INotificationHandler<SubscriptionSignals.Renewed>
{
    public async ValueTask Handle(SubscriptionSignals.Updated signal, CancellationToken cancellationToken)
    {
        await historyRepository.UpdateType(
            signal.OriginalType,
            signal.Subscription.Type,
            signal.Subscription.Id,
            signal.Subscription.Price,
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
