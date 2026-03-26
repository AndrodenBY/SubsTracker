using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Filter;
using SubsTracker.DAL.Entities;
using SubsTracker.Domain.Pagination;

namespace SubsTracker.BLL.Interfaces;

public interface IGroupService
{
    Task<GroupDto?> GetFullInfoById(Guid id, CancellationToken cancellationToken);
    Task<PaginatedList<GroupDto>> GetAll(GroupFilterDto? filter, PaginationParameters? paginationParameters, CancellationToken cancellationToken);
    Task<GroupDto> Create(Guid userId, CreateGroupDto createDto, CancellationToken cancellationToken);
    Task<GroupDto> Update(Guid updateId, UpdateGroupDto updateDto, CancellationToken cancellationToken);
    Task<GroupDto> ShareSubscription(Guid groupId, Guid subscriptionId, CancellationToken cancellationToken);
    Task<GroupDto> UnshareSubscription(Guid groupId, Guid subscriptionId, CancellationToken cancellationToken);
    new Task<bool> Delete(Guid id, CancellationToken cancellationToken);
}
