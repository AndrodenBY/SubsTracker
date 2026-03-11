using SubsTracker.DAL.Entities;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.Domain.Exceptions;

namespace SubsTracker.BLL.Helpers.Policy;

public static class SubscriptionPolicyChecker
{
    public static async Task<(SubscriptionEntity subscription, UserEntity user)> GetValidatedSubscription(
        IUserRepository userRepository, 
        ISubscriptionRepository subscriptionRepository, 
        string identityId, 
        Guid subscriptionId, 
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdentityId(identityId, cancellationToken)
                   ?? throw new UnknownIdentifierException($"User {identityId} not found");

        var subscription = await subscriptionRepository.GetById(subscriptionId, cancellationToken)
                           ?? throw new UnknownIdentifierException($"Subscription {subscriptionId} not found");
        
        if (!subscription.UserId.HasValue || subscription.UserId.Value != user.Id)
        {
            throw new ForbiddenException($"User {user.Id} does not own subscription {subscriptionId}");
        }
        
        return (subscription, user);
    }
    
    public static async Task PreventSubscriptionDuplication(
        ISubscriptionRepository subscriptionRepository, 
        Guid userId, 
        string subscriptionName, 
        CancellationToken cancellationToken)
    {
        var existingSub = await subscriptionRepository.GetByPredicate(
            subscription => subscription.UserId == userId && subscription.Name == subscriptionName && subscription.Active, 
            cancellationToken);
        
        if (existingSub is not null)
        {
            throw new PolicyViolationException($"A subscription named '{subscriptionName}' already exists for this user.");
        }
    }
}
