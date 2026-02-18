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
    public async ValueTask Handle(SubscriptionUpdatedSignal signal, CancellationToken cancellationToken)
    {
        await historyRepository.UpdateType(
            signal.OriginalType,
            signal.Subscription.Type,
            signal.Subscription.Id,
            signal.Subscription.Price,
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
