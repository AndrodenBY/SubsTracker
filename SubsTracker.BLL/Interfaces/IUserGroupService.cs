using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.DAL.Models.User;
using SubsTracker.Domain.Filter;
using SubsTracker.Domain.Interfaces;

namespace SubsTracker.BLL.Interfaces;

public interface IUserGroupService : IService<UserGroup, UserGroupDto, CreateUserGroupDto, UpdateUserGroupDto, UserGroupFilter>
{
    Task<IEnumerable<UserGroupDto>> GetAll(UserGroupFilter? filter, CancellationToken cancellationToken);
    Task<bool> LeaveGroup(Guid groupId, Guid userId, CancellationToken cancellationToken);
    Task<GroupMemberDto> JoinGroup(CreateGroupMemberDto createDto, CancellationToken cancellationToken);
    Task<UserGroupDto> ShareSubscription(Guid groupId, Guid subscriptionId, CancellationToken cancellationToken);
    Task<UserGroupDto> UnshareSubscription(Guid groupId, Guid subscriptionId, CancellationToken cancellationToken);
}