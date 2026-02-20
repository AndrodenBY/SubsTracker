using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using LinqKit;
using SubsTracker.DAL.Entities;
using SubsTracker.Domain.Filter;

namespace SubsTracker.BLL.Helpers.Filters;

[ExcludeFromCodeCoverage]
public static class GroupFilterHelper
{
    public static Expression<Func<GroupEntity, bool>> CreatePredicate(GroupFilterDto? filter)
    {
        var predicate = PredicateBuilder.New<GroupEntity>(true);

        predicate = FilterHelper.AddFilterCondition<GroupEntity>(
            predicate,
            filter?.Name,
            group => group.Name.ToLower().Contains(filter.Name!.ToLower())
        );

        return predicate;
    }
}
