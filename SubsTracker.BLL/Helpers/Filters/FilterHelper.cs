using System.Linq.Expressions;
using LinqKit;

namespace SubsTracker.BLL.Helpers.Filters;

public static class FilterHelper
{
    public static Expression<Func<TModel, bool>> AddFilterCondition<TModel, TValue>(
        Expression<Func<TModel, bool>> predicate,
        TValue? filterValue,
        Expression<Func<TModel, bool>> expression) where TValue : struct
    {
        return filterValue.HasValue ? predicate.And(expression) : predicate;
    }

    public static Expression<Func<TModel, bool>> AddFilterCondition<TModel>(
        Expression<Func<TModel, bool>> predicate,
        string? filterValue,
        Expression<Func<TModel, bool>> expression)
    {
        return !string.IsNullOrWhiteSpace(filterValue) ? predicate.And(expression) : predicate;
    }
}
