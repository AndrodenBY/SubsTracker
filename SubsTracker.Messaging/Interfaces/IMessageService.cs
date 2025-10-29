using SubsTracker.Messaging.Contracts;

namespace SubsTracker.Messaging.Interfaces;

public interface IMessageService
{
    Task NotifySubscriptionCanceled(SubscriptionCanceledEvent canceledSubscription,
        CancellationToken cancellationToken);

    Task NotifySubscriptionRenewed(SubscriptionRenewedEvent renewedSubscription, CancellationToken cancellationToken);
    Task NotifyMemberChangedRole(MemberChangedRoleEvent member, CancellationToken cancellationToken);
    Task NotifyMemberLeftGroup(MemberLeftGroupEvent leftMember, CancellationToken cancellationToken);
}
