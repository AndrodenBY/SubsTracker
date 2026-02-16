using SubsTracker.Domain.Constants;

namespace SubsTracker.Domain.Pagination;

public record PaginationParameters(int PageNumber = PaginationConstants.DefaultPageNumber, int PageSize = PaginationConstants.DefaultPageSize);
