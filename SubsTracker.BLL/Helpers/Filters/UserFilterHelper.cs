using System.Linq.Expressions;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using SubsTracker.DAL.Entities;
using SubsTracker.Domain.Filter;

namespace SubsTracker.BLL.Helpers.Filters;

public static class UserFilterHelper
{
    public static Expression<Func<UserEntity, bool>> CreatePredicate(UserFilterDto? filter)
    {
        var expression = PredicateBuilder.New<UserEntity>(true);

        if (filter is null)
        {
            return expression;
        }
        
        expression = FilterHelper.AddFilterCondition<UserEntity>(
            expression,
            filter.FirstName,
            user => EF.Functions.Like(user.FirstName, $"%{filter.FirstName}%")
        );

        expression = FilterHelper.AddFilterCondition<UserEntity>(
            expression,
            filter.LastName,
            user => user.LastName != null && EF.Functions.Like(user.LastName, $"%{filter.LastName}%")
        );

        expression = FilterHelper.AddFilterCondition<UserEntity>(
            expression,
            filter.Email,
            user => user.Email.ToLower().Equals(filter.Email!.ToLower())
        );

        return expression;
    }
}
