using System.Linq.Expressions;
using LinqKit;
using SubsTracker.DAL.Models.User;
using SubsTracker.Domain.Filter;

namespace SubsTracker.BLL.Helpers.Filters;

public static class UserGroupFilterHelper
{
    public static Expression<Func<UserGroup, bool>> CreatePredicate(UserGroupFilterDto filter)
    {
        var predicate = PredicateBuilder.New<UserGroup>(true);

        predicate = FilterHelper.AddFilterCondition<UserGroup>(
            predicate, 
            filter.Name, 
            group => group.Name.Contains(filter.Name!, StringComparison.OrdinalIgnoreCase)
        );

        return predicate;
    }
}
