using DispatchR.Abstractions.Notification;
using SubsTracker.BLL.Handlers.Signals;
using SubsTracker.BLL.Handlers.Signals.Subscription;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.Domain.Enums;

namespace SubsTracker.BLL.Handlers.History;

public class SubscriptionHistoryHandler(ISubscriptionHistoryRepository historyRepository)
    : INotificationHandler<SubscriptionUpdatedSignal>, 
        INotificationHandler<SubscriptionCanceledSignal>
{
    public async ValueTask Handle(SubscriptionUpdatedSignal notification, CancellationToken cancellationToken)
    {
        await historyRepository.UpdateType(
            notification.OriginalType,
            notification.Subscription.Type,
            notification.Subscription.Id,
            notification.Subscription.Price,
            cancellationToken);
    }
    
    public async ValueTask Handle(SubscriptionCanceledSignal signal, CancellationToken cancellationToken)
    {
        await historyRepository.Create(
            signal.Subscription.Id, 
            SubscriptionAction.Cancel, 
            null, 
            cancellationToken);
    }
}
