using AutoMapper;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Interfaces;
using SubsTracker.DAL.Models.User;
using SubsTracker.Domain.Exceptions;
using SubsTracker.Domain.Interfaces;

namespace SubsTracker.BLL.Services.User;

public class GroupMemberService(IRepository<GroupMember> repository, IMapper mapper) 
    : ServiceBase<GroupMember, GroupMemberDto, CreateGroupMemberDto, UpdateGroupMemberDto>(repository, mapper), IGroupMemberService
{
    public async Task<bool> LeaveGroup(Guid groupId, Guid userId, GroupMemberDto memberDto, CancellationToken cancellationToken)
    {
        if (memberDto.UserId != userId)
        {
            throw new ValidationException($"Expected user with id {memberDto.UserId}, but received id {userId}");
        }
        if (memberDto.GroupId != groupId)
        {
            throw new ValidationException($"Expected group with id {memberDto.GroupId}, but received id {groupId}");
        }
        
        return await Delete(memberDto.Id, cancellationToken);
    }
}