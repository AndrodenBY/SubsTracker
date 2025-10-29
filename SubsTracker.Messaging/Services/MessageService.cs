using MassTransit;
using Microsoft.Extensions.Logging;
using SubsTracker.Messaging.Contracts;
using SubsTracker.Messaging.Interfaces;

namespace SubsTracker.Messaging.Services;

public class MessageService(IPublishEndpoint publishEndpoint, ILogger<MessageService> logger) : IMessageService
{
    public Task NotifySubscriptionCanceled(SubscriptionCanceledEvent canceledSubscription,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to publish SubscriptionCanceledEvent for {SubscriptionId}",
            canceledSubscription.Id);

        var publishedNotification = publishEndpoint.Publish(canceledSubscription, cancellationToken);
        logger.LogInformation("Successfully published SubscriptionCanceledEvent for {SubscriptionId}",
            canceledSubscription.Id);

        return publishedNotification;
    }

    public Task NotifySubscriptionRenewed(SubscriptionRenewedEvent renewedSubscription,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to publish SubscriptionRenewedEvent for {SubscriptionId}",
            renewedSubscription.Id);

        var publishedNotification = publishEndpoint.Publish(renewedSubscription, cancellationToken);
        logger.LogInformation("Successfully published SubscriptionRenewedEvent for {SubscriptionId}",
            renewedSubscription.Id);

        return publishedNotification;
    }

    public Task NotifyMemberChangedRole(MemberChangedRoleEvent member, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to publish MemberChangedRoleEvent for {MemberId}", member.Id);

        var publishedNotification = publishEndpoint.Publish(member, cancellationToken);
        logger.LogInformation("Successfully published MemberChangedRoleEvent for {MemberId}", member.Id);

        return publishedNotification;
    }

    public Task NotifyMemberLeftGroup(MemberLeftGroupEvent leftMember, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to publish MemberLeftGroupEvent for {MemberId}", leftMember.Id);

        var publishNotification = publishEndpoint.Publish(leftMember, cancellationToken);
        logger.LogInformation("Successfully published MemberLeftGroupEvent for {MemberId}", leftMember.Id);

        return publishNotification;
    }
}
