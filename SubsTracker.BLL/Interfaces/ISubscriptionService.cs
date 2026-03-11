using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.BLL.Filter;
using SubsTracker.DAL.Entities;
using SubsTracker.Domain.Pagination;

namespace SubsTracker.BLL.Interfaces
{
    public interface ISubscriptionService : IService<SubscriptionEntity, SubscriptionDto, CreateSubscriptionDto, UpdateSubscriptionDto, SubscriptionFilterDto>
    {
        Task<SubscriptionDto?> GetUserInfoById(Guid id, CancellationToken cancellationToken);
        Task<PaginatedList<SubscriptionDto>> GetAll(SubscriptionFilterDto? filter, PaginationParameters? paginationParameters, CancellationToken cancellationToken);
        Task<SubscriptionDto> Create(string identityId, CreateSubscriptionDto createDto, CancellationToken cancellationToken);
        Task<SubscriptionDto> Update(string identityId, UpdateSubscriptionDto updateDto, CancellationToken cancellationToken);
        Task<SubscriptionDto> CancelSubscription(string identityId, Guid subscriptionId, CancellationToken cancellationToken);
        Task<SubscriptionDto> RenewSubscription(Guid subscriptionId, int monthsToRenew, CancellationToken cancellationToken);
        Task<List<SubscriptionDto>> GetUpcomingBills(string identityId, CancellationToken cancellationToken);
    }
}
