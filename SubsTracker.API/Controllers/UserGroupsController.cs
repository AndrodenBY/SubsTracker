using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SubsTracker.API.ViewModel.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Interfaces.User;
using SubsTracker.Domain.Filter;

namespace SubsTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserGroupsController(
    IUserGroupService service,
    IMapper mapper
    ) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<UserGroupViewModel> GetById(Guid id, CancellationToken cancellationToken)
    {
        var getById = await service.GetById(id, cancellationToken);
        return mapper.Map<UserGroupViewModel>(getById);
    }

    [HttpGet]
    public async Task<List<UserGroupViewModel>> GetAll([FromQuery] UserGroupFilterDto? filterDto, CancellationToken cancellationToken)
    {
        var getAll = await service.GetAll(filterDto, cancellationToken);
        return mapper.Map<List<UserGroupViewModel>>(getAll);
    }

    [HttpGet]
    public async Task<List<GroupMemberViewModel>> GetAllMembers([FromQuery] GroupMemberFilterDto? filterDto, CancellationToken cancellationToken)
    {
        var entities = await service.GetAll(filterDto, cancellationToken);
        return mapper.Map<List<GroupMemberViewModel>>(entities);
    }

    [HttpPost("{userId:guid}")]
    public async Task<UserGroupViewModel> Create(Guid userId, [FromBody] CreateUserGroupDto createDto, CancellationToken cancellationToken)
    {
        var create = await service.Create(userId, createDto, cancellationToken);
        return mapper.Map<UserGroupViewModel>(create);
    }

    [HttpPut("{id:guid}")]
    public async Task<UserGroupViewModel> Update(Guid id, [FromBody] UpdateUserGroupDto updateDto, CancellationToken cancellationToken)
    {
        var update = await service.Update(id, updateDto, cancellationToken);
        return mapper.Map<UserGroupViewModel>(update);
    }

    [HttpDelete("{id:guid}")]
    public async Task Delete(Guid id, CancellationToken cancellationToken)
    {
        await service.Delete(id, cancellationToken);
    }

    [HttpPost("join")]
    public async Task JoinGroup([FromBody] CreateGroupMemberDto createDto, CancellationToken cancellationToken)
    {
        await service.JoinGroup(createDto, cancellationToken);
    }

    [HttpDelete("leave")]
    public async Task LeaveGroup([FromQuery] Guid groupId, [FromQuery] Guid userId, CancellationToken cancellationToken)
    {
        await service.LeaveGroup(groupId, userId, cancellationToken);
    }

    [HttpPut("members/{memberId:guid}/moderator")]
    public async Task<GroupMemberViewModel> MakeModerator(Guid memberId, CancellationToken cancellationToken)
    {
        var newModerator = await service.MakeModerator(memberId, cancellationToken);
        return mapper.Map<GroupMemberViewModel>(newModerator);
    }

    [HttpPost("share")]
    public async Task<UserGroupViewModel> ShareSubscription([FromQuery] Guid groupId, [FromQuery] Guid subscriptionId, CancellationToken cancellationToken)
    {
        var updatedGroup = await service.ShareSubscription(groupId, subscriptionId, cancellationToken);
        return mapper.Map<UserGroupViewModel>(updatedGroup);
    }

    [HttpPost("unshare")]
    public async Task<UserGroupViewModel> UnshareSubscription([FromQuery] Guid groupId, [FromQuery] Guid subscriptionId, CancellationToken cancellationToken)
    {
        var updatedGroup = await service.UnshareSubscription(groupId, subscriptionId, cancellationToken);
        return mapper.Map<UserGroupViewModel>(updatedGroup);
    }
}
