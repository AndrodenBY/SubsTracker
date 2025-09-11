using SubsTracker.BLL.DTOs.Filter;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.DAL.Models.User;

namespace SubsTracker.BLL.Interfaces.user;

public interface IUserGroupService : 
    IService<UserGroup, UserGroupDto, CreateUserGroupDto, UpdateUserGroupDto, UserGroupFilterDto>
{
    Task<IEnumerable<UserGroupDto>> GetAll(UserGroupFilterDto? filter, CancellationToken cancellationToken);
    Task<bool> LeaveGroup(Guid groupId, Guid userId, CancellationToken cancellationToken);
    Task<GroupMemberDto> JoinGroup(CreateGroupMemberDto createDto, CancellationToken cancellationToken);
    Task<UserGroupDto> ShareSubscription(Guid groupId, Guid subscriptionId, CancellationToken cancellationToken);
    Task<UserGroupDto> UnshareSubscription(Guid groupId, Guid subscriptionId, CancellationToken cancellationToken);
}
