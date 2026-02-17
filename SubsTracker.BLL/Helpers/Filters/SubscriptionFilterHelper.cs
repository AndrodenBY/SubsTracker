using System.Linq.Expressions;
using LinqKit;
using SubsTracker.DAL.Models.Subscription;
using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Filter;

namespace SubsTracker.BLL.Helpers.Filters;

public static class SubscriptionFilterHelper
{
    public static Expression<Func<Subscription, bool>> CreatePredicate(SubscriptionFilterDto? filter)
    {
        var predicate = PredicateBuilder.New<Subscription>(true);

        if (filter is null)
        {
            return predicate;
        }

        predicate = FilterHelper.AddFilterCondition<Subscription>(
            predicate,
            filter.Name,
            subscription => subscription.Name.ToLower().Contains(filter.Name!.ToLower())
        );

        predicate = FilterHelper.AddFilterCondition<Subscription, Guid>(
            predicate,
            filter.Id,
            subscription => subscription.Id == filter.Id
        );

        predicate = FilterHelper.AddFilterCondition<Subscription, Guid>(
            predicate,
            filter.UserId,
            subscription => subscription.UserId == filter.UserId
        );

        predicate = FilterHelper.AddFilterCondition<Subscription, decimal>(
            predicate,
            filter.Price,
            subscription => subscription.Price == filter.Price!.Value
        );

        predicate = FilterHelper.AddFilterCondition<Subscription, SubscriptionType>(
            predicate,
            filter.Type,
            subscription => subscription.Type == filter.Type!.Value
        );

        predicate = FilterHelper.AddFilterCondition<Subscription, SubscriptionContent>(
            predicate,
            filter.Content,
            subscription => subscription.Content == filter.Content!.Value
        );
        
        predicate = FilterHelper.AddFilterCondition<Subscription, bool>(
            predicate,
            filter.Active,
            subscription => subscription.Active == filter.Active
        );

        return predicate;
    }
}
