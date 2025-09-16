using System.Linq.Expressions;
using LinqKit;
using SubsTracker.DAL.Models.User;
using SubsTracker.Domain.Filter;

namespace SubsTracker.BLL.Helpers.Filters;

public static class UserFilterHelper
{
    public static Expression<Func<User, bool>> CreatePredicate(UserFilterDto filter)
    {
        var predicate = PredicateBuilder.New<User>(true);

        predicate = FilterHelper.AddFilterCondition<User>(
            predicate, 
            filter.FirstName, 
            user => user.FirstName.Contains(filter.FirstName!, StringComparison.OrdinalIgnoreCase)
        );

        predicate = FilterHelper.AddFilterCondition<User>(
            predicate, 
            filter.LastName, 
            user => user.LastName != null && user.LastName.Contains(filter.LastName!, StringComparison.OrdinalIgnoreCase)
        );

        predicate = FilterHelper.AddFilterCondition<User>(
            predicate, 
            filter.Email, 
            user => user.Email.Equals(filter.Email, StringComparison.OrdinalIgnoreCase)
        );

        return predicate;
    }
}
