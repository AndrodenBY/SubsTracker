using System.Linq.Expressions;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using SubsTracker.BLL.Filter;
using SubsTracker.DAL.Entities;
using SubsTracker.Domain.Enums;

namespace SubsTracker.BLL.Helpers.Filters;

public static class SubscriptionHistoryFilterHelper
{
    public static Expression<Func<SubscriptionHistory, bool>> CreatePredicate(SubscriptionHistoryFilter? filter)
    {
        var expression = PredicateBuilder.New<SubscriptionHistory>(true);

        if (filter is null)
        {
            return expression;
        }
        
        expression = FilterHelper.AddFilterCondition<SubscriptionHistory>(
            expression,
            filter.SubscriptionName,
            subscriptionHistory => EF.Functions.Like(subscriptionHistory.Subscription!.Name, $"%{filter.SubscriptionName}%")
        );
        
        expression = FilterHelper.AddFilterCondition<SubscriptionHistory, bool>(
            expression,
            filter.SubscriptionActive,
            subscriptionHistory => subscriptionHistory.Subscription!.Active == filter.SubscriptionActive
        );
        
        expression = FilterHelper.AddFilterCondition<SubscriptionHistory, SubscriptionContent>(
            expression,
            filter.SubscriptionContent,
            subscriptionHistory => subscriptionHistory.Subscription!.Content == filter.SubscriptionContent
        );
        
        expression = FilterHelper.AddFilterCondition<SubscriptionHistory, SubscriptionType>(
            expression,
            filter.SubscriptionType,
            subscriptionHistory => subscriptionHistory.Subscription!.Type == filter.SubscriptionType
        );

        expression = FilterHelper.AddFilterCondition<SubscriptionHistory, decimal>(
            expression,
            filter.PricePaid,
            subscriptionHistory => subscriptionHistory.PricePaid == filter.PricePaid
        );

        expression = FilterHelper.AddFilterCondition<SubscriptionHistory, SubscriptionAction>(
            expression,
            filter.Action,
            subscriptionHistory => subscriptionHistory.Action == filter.Action
        );

        expression = FilterHelper.AddFilterCondition<SubscriptionHistory, DateTime>(
            expression,
            filter.CreatedAt,
            subscriptionHistory => subscriptionHistory.CreatedAt == filter.CreatedAt!.Value
        );

        return expression;
    }
}
