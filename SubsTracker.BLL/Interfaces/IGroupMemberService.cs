using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.DAL.Models.User;
using SubsTracker.Domain.Filter;
using SubsTracker.Domain.Interfaces;

namespace SubsTracker.BLL.Interfaces;

public interface IGroupMemberService : IService<GroupMember, GroupMemberDto, CreateGroupMemberDto, UpdateGroupMemberDto, GroupMemberFilter>
{
    Task<IEnumerable<GroupMemberDto>> GetAll(GroupMemberFilter? filter, CancellationToken cancellationToken);
}