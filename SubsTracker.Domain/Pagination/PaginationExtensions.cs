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
        
        var pageCount = totalCount == 0 
            ? 0 
            : (int)Math.Ceiling(totalCount / (double)appliedPageSize);
        
        var appliedPageNumber = totalCount == 0 
            ? 0 
            : Math.Max(1, pageNumber);

        return new PaginatedList<TItem>(
            source.ToList(), 
            appliedPageNumber, 
            appliedPageSize, 
            pageCount, 
            totalCount);
    }
}
