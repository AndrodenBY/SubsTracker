using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.DAL.Models.User;
using SubsTracker.Domain.Interfaces;

namespace SubsTracker.BLL.Interfaces;

public interface IGroupMemberService : IService<GroupMember, GroupMemberDto, CreateGroupMemberDto, UpdateGroupMemberDto>
{
    Task<bool> LeaveGroup(Guid groupId, Guid userId, GroupMemberDto memberDto, CancellationToken cancellationToken);
}