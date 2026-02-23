using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.DAL.Entities;
using SubsTracker.Domain.Filter;
using SubsTracker.Domain.Pagination;

namespace SubsTracker.BLL.Interfaces;

public interface IGroupService : IService<GroupEntity, GroupDto, CreateGroupDto, UpdateGroupDto, GroupFilterDto>
{
    Task<GroupDto?> GetFullInfoById(Guid id, CancellationToken cancellationToken);
    Task<PaginatedList<GroupDto>> GetAll(GroupFilterDto? filter, PaginationParameters? paginationParameters, CancellationToken cancellationToken);
    Task<GroupDto> Create(string auth0Id, CreateGroupDto createDto, CancellationToken cancellationToken);
    Task<GroupDto> ShareSubscription(Guid groupId, Guid subscriptionId, CancellationToken cancellationToken);
    Task<GroupDto> UnshareSubscription(Guid groupId, Guid subscriptionId, CancellationToken cancellationToken);
    new Task<bool> Delete(Guid id, CancellationToken cancellationToken);
}
