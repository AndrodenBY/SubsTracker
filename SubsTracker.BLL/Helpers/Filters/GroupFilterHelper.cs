
using System.Linq.Expressions;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using SubsTracker.DAL.Entities;
using SubsTracker.Domain.Filter;

namespace SubsTracker.BLL.Helpers.Filters;


public static class GroupFilterHelper
{
    public static Expression<Func<GroupEntity, bool>> CreatePredicate(GroupFilterDto? filter)
    {
        var expression = PredicateBuilder.New<GroupEntity>(true);

        if (filter is null)
        {
            return expression;
        }
        
        expression = FilterHelper.AddFilterCondition<GroupEntity>(
            expression,
            filter.Name,
            group => EF.Functions.Like(group.Name, $"%{filter.Name}%")
        );

        return expression;
    }
}
