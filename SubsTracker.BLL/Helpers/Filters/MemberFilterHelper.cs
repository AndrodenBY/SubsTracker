using System.Linq.Expressions;
using LinqKit;
using SubsTracker.DAL.Entities;
using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Filter;

namespace SubsTracker.BLL.Helpers.Filters;

public static class MemberFilterHelper
{
    public static Expression<Func<MemberEntity, bool>> CreatePredicate(MemberFilterDto? filter)
    {
        var expression = PredicateBuilder.New<MemberEntity>(true);

        if (filter is null)
        {
            return expression;
        }
        
        expression = FilterHelper.AddFilterCondition<MemberEntity, Guid>(
            expression,
            filter.Id,
            member => member.Id == filter.Id!.Value
        );

        expression = FilterHelper.AddFilterCondition<MemberEntity, MemberRole>(
            expression,
            filter.Role,
            member => member.Role == filter.Role!.Value
        );

        return expression;
    }
}
