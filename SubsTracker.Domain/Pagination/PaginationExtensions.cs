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
        var appliedPageSize = pageSize > 0 
            ? pageSize 
            : PaginationConstants.DefaultPageSize;
        
        var pageCount = (int)Math.Ceiling(totalCount / (double)appliedPageSize);
        
        var appliedPageNumber = totalCount == 0 
            ? 0 
            : Math.Clamp(pageNumber, 1, Math.Max(1, pageCount));

        return new PaginatedList<TItem>(
            source.ToList(), 
            appliedPageNumber, 
            appliedPageSize, 
            totalCount);
    }
}
