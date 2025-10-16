using MassTransit;
using SubsTracker.DAL.Models.Subscription;
using SubsTracker.DAL.Models.User;
using SubsTracker.Messaging.Contracts;
using SubsTracker.Messaging.Interfaces;

namespace SubsTracker.Messaging.Services;

public class MessageService(IPublishEndpoint publishEndpoint) : IMessageService
{
    public Task NotifySubscriptionCanceled(Subscription canceledSubscription, CancellationToken cancellationToken) 
    {
        return publishEndpoint.Publish<SubscriptionCanceledEvent>(new (
            canceledSubscription.Id, 
            canceledSubscription.Name, 
            canceledSubscription.User.Id, 
            canceledSubscription.User.Email
            ), cancellationToken);
    }

    public Task NotifySubscriptionRenewed(Subscription renewedSubscription, CancellationToken cancellationToken)
    {
        return publishEndpoint.Publish<SubscriptionRenewedEvent>(new(
            renewedSubscription.Id, 
            renewedSubscription.User.Id, 
            renewedSubscription.DueDate, 
            renewedSubscription.User.Email
            ), cancellationToken);
    }
    
    public Task NotifyMemberChangedRole(GroupMember member, CancellationToken cancellationToken)
    {
        return publishEndpoint.Publish<MemberChangedRoleEvent>(new (
            member.UserId, 
            member.GroupId,
            member.Role,
            member.Group.Name, 
            member.User.Email
        ), cancellationToken);
    }

    public Task NotifyMemberLeftGroup(GroupMember leftMember, CancellationToken cancellationToken)
    {
        return publishEndpoint.Publish<MemberLeftGroupEvent>(new (
            leftMember.UserId, 
            leftMember.GroupId, 
            leftMember.Group.Name, 
            leftMember.User.Email
            ), cancellationToken);
    }
}
