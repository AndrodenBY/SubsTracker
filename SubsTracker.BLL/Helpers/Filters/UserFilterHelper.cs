using System.Linq.Expressions;
using LinqKit;
using SubsTracker.DAL.Entities;
using SubsTracker.Domain.Filter;

namespace SubsTracker.BLL.Helpers.Filters;

public static class UserFilterHelper
{
    public static Expression<Func<UserEntity, bool>> CreatePredicate(UserFilterDto? filter)
    {
        var predicate = PredicateBuilder.New<UserEntity>(true);

        predicate = FilterHelper.AddFilterCondition<UserEntity>(
            predicate,
            filter?.FirstName,
            user => user.FirstName.ToLower().Contains(filter.FirstName!.ToLower())
        );

        predicate = FilterHelper.AddFilterCondition<UserEntity>(
            predicate,
            filter?.LastName,
            user => user.LastName != null && user.LastName.ToLower().Contains(filter.LastName!.ToLower())
        );

        predicate = FilterHelper.AddFilterCondition<UserEntity>(
            predicate,
            filter?.Email,
            user => user.Email.ToLower().Equals(filter.Email!.ToLower())
        );

        return predicate;
    }
}
