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
        var predicate = PredicateBuilder.New<MemberEntity>(true);

        if (filter is null)
        {
            return predicate;
        }
        
        predicate = FilterHelper.AddFilterCondition<MemberEntity, Guid>(
            predicate,
            filter.Id,
            member => member.Id == filter.Id!.Value
        );

        predicate = FilterHelper.AddFilterCondition<MemberEntity, MemberRole>(
            predicate,
            filter.Role,
            member => member.Role == filter.Role!.Value
        );

        return predicate;
    }
}
