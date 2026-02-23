
using System.Linq.Expressions;
using LinqKit;

namespace SubsTracker.BLL.Helpers.Filters;

public static class FilterHelper
{
    public static Expression<Func<TEntity, bool>> AddFilterCondition<TEntity, TValue>(
        Expression<Func<TEntity, bool>> predicate,
        TValue? filterValue,
        Expression<Func<TEntity, bool>> condition) where TValue : struct
    {
        return filterValue.HasValue 
            ? predicate.And(condition) 
            : predicate;
    }

    public static Expression<Func<TEntity, bool>> AddFilterCondition<TEntity>(
        Expression<Func<TEntity, bool>> predicate,
        string? filterValue,
        Expression<Func<TEntity, bool>> condition)
    {
        return !string.IsNullOrWhiteSpace(filterValue) 
            ? predicate.And(condition) 
            : condition;
    }
}
