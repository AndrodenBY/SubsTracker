using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.DAL.Models.User;
using SubsTracker.Domain.Filter;

namespace SubsTracker.BLL.Interfaces.User;

public interface IUserGroupService : 
    IService<UserGroup, UserGroupDto, CreateUserGroupDto, UpdateUserGroupDto, UserGroupFilterDto>
{
    Task<List<UserGroupDto>> GetAll(UserGroupFilterDto? filter, CancellationToken cancellationToken);
    Task<List<GroupMemberDto>> GetAll(GroupMemberFilterDto? filter, CancellationToken cancellationToken);
    Task<GroupMemberDto> JoinGroup(CreateGroupMemberDto createDto, CancellationToken cancellationToken);
    Task<bool> LeaveGroup(Guid groupId, Guid userId, CancellationToken cancellationToken);
    Task<GroupMemberDto> MakeModerator(Guid memberId, CancellationToken cancellationToken);
    Task<UserGroupDto> ShareSubscription(Guid groupId, Guid subscriptionId, CancellationToken cancellationToken);
    Task<UserGroupDto> UnshareSubscription(Guid groupId, Guid subscriptionId, CancellationToken cancellationToken);
}
