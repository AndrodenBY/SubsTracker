using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.DAL.Models.User;
using SubsTracker.Domain.Filter;

namespace SubsTracker.BLL.Interfaces.User;

public interface IUserGroupService :
    IService<UserGroup, UserGroupDto, CreateUserGroupDto, UpdateUserGroupDto, UserGroupFilterDto>
{
    Task<UserGroupDto?> GetById(Guid id, CancellationToken cancellationToken);
    Task<List<UserGroupDto>> GetAll(UserGroupFilterDto? filter, CancellationToken cancellationToken);
    Task<UserGroupDto> Create(Guid userId, CreateUserGroupDto createDto, CancellationToken cancellationToken);
    Task<UserGroupDto> Update(Guid updateId, UpdateUserGroupDto updateDto, CancellationToken cancellationToken);
    Task<UserGroupDto> ShareSubscription(Guid groupId, Guid subscriptionId, CancellationToken cancellationToken);
    Task<UserGroupDto> UnshareSubscription(Guid groupId, Guid subscriptionId, CancellationToken cancellationToken);
    Task<bool> Delete(Guid id, CancellationToken cancellationToken);
}
