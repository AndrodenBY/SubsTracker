using SubsTracker.DAL.Models.Subscription;
using SubsTracker.DAL.Models.User;

namespace SubsTracker.Messaging.Interfaces;

public interface IMessageService
{
    Task NotifySubscriptionCanceled(Subscription canceledSubscription, CancellationToken cancellationToken);
    Task NotifySubscriptionRenewed(Subscription renewedSubscription, CancellationToken cancellationToken);
    Task NotifyMemberChangedRole(GroupMember member, CancellationToken cancellationToken);
    Task NotifyMemberLeftGroup(GroupMember leftMember, CancellationToken cancellationToken);
}
