using System.Linq.Expressions;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using SubsTracker.DAL.Entities;
using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Filter;

namespace SubsTracker.BLL.Helpers.Filters;

public static class SubscriptionFilterHelper
{
    public static Expression<Func<SubscriptionEntity, bool>> CreatePredicate(SubscriptionFilterDto? filter)
    {
        var expression = PredicateBuilder.New<SubscriptionEntity>(true);

        if (filter is null)
        {
            return expression;
        }
        
        expression = FilterHelper.AddFilterCondition<SubscriptionEntity>(
            expression,
            filter.Name,
            subscription => EF.Functions.Like(subscription.Name, $"%{filter.Name}%")
        );

        expression = FilterHelper.AddFilterCondition<SubscriptionEntity, Guid>(
            expression,
            filter.Id,
            subscription => subscription.Id == filter.Id
        );

        expression = FilterHelper.AddFilterCondition<SubscriptionEntity, Guid>(
            expression,
            filter.UserId,
            subscription => subscription.UserId == filter.UserId
        );

        expression = FilterHelper.AddFilterCondition<SubscriptionEntity, decimal>(
            expression,
            filter.Price,
            subscription => subscription.Price == filter.Price!.Value
        );

        expression = FilterHelper.AddFilterCondition<SubscriptionEntity, SubscriptionType>(
            expression,
            filter.Type,
            subscription => subscription.Type == filter.Type!.Value
        );

        expression = FilterHelper.AddFilterCondition<SubscriptionEntity, SubscriptionContent>(
            expression,
            filter.Content,
            subscription => subscription.Content == filter.Content!.Value
        );
        
        expression = FilterHelper.AddFilterCondition<SubscriptionEntity, bool>(
            expression,
            filter.Active,
            subscription => subscription.Active == filter.Active
        );

        return expression;
    }
}
