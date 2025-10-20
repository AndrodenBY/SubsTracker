using MassTransit;
using SubsTracker.Messaging.Contracts;
using SubsTracker.Messaging.Interfaces;

namespace SubsTracker.Messaging.Services;

public class MessageService(IPublishEndpoint publishEndpoint) : IMessageService
{
    public Task NotifySubscriptionCanceled(SubscriptionCanceledEvent canceledSubscription, CancellationToken cancellationToken) 
    {
        return publishEndpoint.Publish(canceledSubscription, cancellationToken);
    }

    public Task NotifySubscriptionRenewed(SubscriptionRenewedEvent renewedSubscription, CancellationToken cancellationToken)
    {
        return publishEndpoint.Publish(renewedSubscription, cancellationToken);
    }
    
    public Task NotifyMemberChangedRole(MemberChangedRoleEvent member, CancellationToken cancellationToken)
    {
        return publishEndpoint.Publish(member, cancellationToken);
    }

    public Task NotifyMemberLeftGroup(MemberLeftGroupEvent leftMember, CancellationToken cancellationToken)
    {
        return publishEndpoint.Publish(leftMember, cancellationToken);
    }
}
