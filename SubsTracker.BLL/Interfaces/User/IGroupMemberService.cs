using SubsTracker.BLL.DTOs.Filter;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.DAL.Models.User;

namespace SubsTracker.BLL.Interfaces.User;

public interface IGroupMemberService : 
    IService<GroupMember, GroupMemberDto, CreateGroupMemberDto, UpdateGroupMemberDto, GroupMemberFilterDto>
{
    Task<IEnumerable<GroupMemberDto>> GetAll(GroupMemberFilterDto? filter, CancellationToken cancellationToken);
}
