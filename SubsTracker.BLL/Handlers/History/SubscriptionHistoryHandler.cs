using DispatchR.Abstractions.Notification;
using SubsTracker.BLL.Handlers.Notifications;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.Domain.Enums;

namespace SubsTracker.BLL.Handlers.History;

public class SubscriptionHistoryHandler(ISubscriptionHistoryRepository historyRepository)
    : INotificationHandler<SubscriptionUpdatedNotification>, 
        INotificationHandler<SubscriptionCanceledNotification>
{
    public async ValueTask Handle(SubscriptionUpdatedNotification notification, CancellationToken cancellationToken)
    {
        await historyRepository.UpdateType(
            notification.OriginalType,
            notification.Subscription.Type,
            notification.Subscription.Id,
            notification.Subscription.Price,
            cancellationToken);
    }
    
    public async ValueTask Handle(SubscriptionCanceledNotification notification, CancellationToken cancellationToken)
    {
        await historyRepository.Create(
            notification.Subscription.Id, 
            SubscriptionAction.Cancel, 
            null, 
            cancellationToken);
    }
}
