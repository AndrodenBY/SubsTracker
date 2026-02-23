
using System.Linq.Expressions;
using LinqKit;
using SubsTracker.DAL.Entities;
using SubsTracker.Domain.Filter;

namespace SubsTracker.BLL.Helpers.Filters;


public static class GroupFilterHelper
{
    public static Expression<Func<GroupEntity, bool>> CreatePredicate(GroupFilterDto? filter)
    {
        var predicate = PredicateBuilder.New<GroupEntity>(true);

        if (filter is null)
        {
            return predicate;
        }
        
        predicate = FilterHelper.AddFilterCondition<GroupEntity>(
            predicate,
            filter.Name,
            group => group.Name.ToLower().Contains(filter.Name!.ToLower())
        );

        return predicate;
    }
}
