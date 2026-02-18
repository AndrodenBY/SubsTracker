using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using LinqKit;
using SubsTracker.DAL.Models.User;
using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Filter;

namespace SubsTracker.BLL.Helpers.Filters;

[ExcludeFromCodeCoverage]
public static class GroupMemberFilterHelper
{
    public static Expression<Func<GroupMember, bool>> CreatePredicate(GroupMemberFilterDto? filter)
    {
        var predicate = PredicateBuilder.New<GroupMember>(true);

        if (filter is null)
        {
            return predicate;
        }

        predicate = FilterHelper.AddFilterCondition<GroupMember, Guid>(
            predicate,
            filter.Id,
            member => member.Id == filter.Id!.Value
        );

        predicate = FilterHelper.AddFilterCondition<GroupMember, MemberRole>(
            predicate,
            filter.Role,
            member => member.Role == filter.Role!.Value
        );

        return predicate;
    }
}
