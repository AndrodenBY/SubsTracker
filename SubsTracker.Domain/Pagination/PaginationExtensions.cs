using SubsTracker.Domain.Constants;

namespace SubsTracker.Domain.Pagination;

public static class PaginationExtensions
{
    public static PaginatedList<TItem> ToPagedList<TItem>(
        this IEnumerable<TItem> source,
        int pageNumber, 
        int pageSize, 
        int totalCount)
    {
        var appliedPageSize = pageSize <= 0 
            ? PaginationConstants.DefaultPageSize 
            : pageSize;
        
        var pageCount = (int)Math.Ceiling(totalCount / (double)appliedPageSize);

        return new PaginatedList<TItem>(source.ToList(), pageNumber, appliedPageSize, pageCount, totalCount);
    }
}
