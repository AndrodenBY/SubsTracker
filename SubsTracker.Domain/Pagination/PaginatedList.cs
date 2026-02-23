namespace SubsTracker.Domain.Pagination;

public record PaginatedList<TItem>(List<TItem> Items, int PageNumber, int PageSize, int TotalCount)
{
    public int PageCount => (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPreviousPage => PageNumber > 1;
    
    public bool HasNextPage => PageNumber < PageCount; 
    
    public PaginatedList<TTarget> MapToPage<TTarget>(Func<TItem, TTarget> mapFunc)
    {
        var mappedItems = Items.Select(mapFunc).ToList();
        return new PaginatedList<TTarget>(mappedItems, PageNumber, PageSize, TotalCount);
    }
}
