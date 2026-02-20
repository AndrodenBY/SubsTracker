using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using LinqKit;

namespace SubsTracker.BLL.Helpers.Filters;

[ExcludeFromCodeCoverage]
public static class FilterHelper
{
    public static Expression<Func<TEntity, bool>> AddFilterCondition<TEntity, TValue>(
        Expression<Func<TEntity, bool>> predicate,
        TValue? filterValue,
        Expression<Func<TEntity, bool>> expression) where TValue : struct
    {
        return filterValue.HasValue ? predicate.And(expression) : predicate;
    }

    public static Expression<Func<TEntity, bool>> AddFilterCondition<TEntity>(
        Expression<Func<TEntity, bool>> predicate,
        string? filterValue,
        Expression<Func<TEntity, bool>> expression)
    {
        return !string.IsNullOrWhiteSpace(filterValue) ? predicate.And(expression) : predicate;
    }
}
