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
            user => user.FirstName.ToLower().Contains(filter.FirstName!.ToLower())
        );

        predicate = FilterHelper.AddFilterCondition<User>(
            predicate,
            filter.LastName,
            user => user.LastName != null && user.LastName.ToLower().Contains(filter.LastName!.ToLower())
        );

        predicate = FilterHelper.AddFilterCondition<User>(
            predicate,
            filter.Email,
            user => user.Email.ToLower().Equals(filter.Email!.ToLower())
        );

        return predicate;
    }
}
