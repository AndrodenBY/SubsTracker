using System.Linq.Expressions;
using LinqKit;
using SubsTracker.DAL.Entities.Subscription;
using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Filter;

namespace SubsTracker.BLL.Helpers.Filters;

public static class SubscriptionFilterHelper
{
    public static Expression<Func<SubscriptionEntity, bool>> CreatePredicate(SubscriptionFilterDto? filter)
    {
        var predicate = PredicateBuilder.New<SubscriptionEntity>(true);

        predicate = FilterHelper.AddFilterCondition<SubscriptionEntity>(
            predicate,
            filter?.Name,
            subscription => subscription.Name.ToLower().Contains(filter.Name!.ToLower())
        );

        predicate = FilterHelper.AddFilterCondition<SubscriptionEntity, Guid>(
            predicate,
            filter.Id,
            subscription => subscription.Id == filter.Id
        );

        predicate = FilterHelper.AddFilterCondition<SubscriptionEntity, Guid>(
            predicate,
            filter.UserId,
            subscription => subscription.UserId == filter.UserId
        );

        predicate = FilterHelper.AddFilterCondition<SubscriptionEntity, decimal>(
            predicate,
            filter.Price,
            subscription => subscription.Price == filter.Price!.Value
        );

        predicate = FilterHelper.AddFilterCondition<SubscriptionEntity, SubscriptionType>(
            predicate,
            filter.Type,
            subscription => subscription.Type == filter.Type!.Value
        );

        predicate = FilterHelper.AddFilterCondition<SubscriptionEntity, SubscriptionContent>(
            predicate,
            filter.Content,
            subscription => subscription.Content == filter.Content!.Value
        );

        return predicate;
    }
}
