using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubsTracker.API.Extension;
using SubsTracker.API.ViewModel.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Interfaces.User;
using SubsTracker.Domain.Filter;

namespace SubsTracker.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserGroupsController(
    IUserGroupService service,
    IGroupMemberService memberService,
    IMapper mapper
) : ControllerBase
{
    /// <summary>
    ///     Retrieves a user group by its ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<UserGroupViewModel> GetById(Guid id, CancellationToken cancellationToken)
    {
        var getById = await service.GetFullInfoById(id, cancellationToken);
        return mapper.Map<UserGroupViewModel>(getById);
    }

    /// <summary>
    ///     Retrieves all user groups with optional filtering
    /// </summary>
    [HttpGet]
    public async Task<List<UserGroupViewModel>> GetAll([FromQuery] UserGroupFilterDto? filterDto, CancellationToken cancellationToken)
    {
        var getAll = await service.GetAll(filterDto, cancellationToken);
        return mapper.Map<List<UserGroupViewModel>>(getAll);
    }

    /// <summary>
    ///     Retrieves all members of a group with optional filtering
    /// </summary>
    [HttpGet("members")]
    public async Task<List<GroupMemberViewModel>> GetAllMembers([FromQuery] GroupMemberFilterDto? filterDto, CancellationToken cancellationToken)
    {
        var entities = await memberService.GetAll(filterDto, cancellationToken);
        return mapper.Map<List<GroupMemberViewModel>>(entities);
    }

    /// <summary>
    ///     Creates a new user group
    /// </summary>
    [HttpPost]
    public async Task<UserGroupViewModel> Create(CreateUserGroupDto createDto, CancellationToken cancellationToken)
    {
        var create = await service.Create(User.GetAuth0IdFromToken(), createDto, cancellationToken);
        return mapper.Map<UserGroupViewModel>(create);
    }

    /// <summary>
    ///     Updates an existing user group
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<UserGroupViewModel> Update(Guid id, [FromBody] UpdateUserGroupDto updateDto,
        CancellationToken cancellationToken)
    {
        var update = await service.Update(id, updateDto, cancellationToken);
        return mapper.Map<UserGroupViewModel>(update);
    }

    /// <summary>
    ///     Deletes a user group by its ID
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task Delete(Guid id, CancellationToken cancellationToken)
    {
        await service.Delete(id, cancellationToken);
    }

    /// <summary>
    ///     Adds a user to a group
    /// </summary>
    [HttpPost("join")]
    public async Task JoinGroup([FromBody] CreateGroupMemberDto createDto, CancellationToken cancellationToken)
    {
        await memberService.JoinGroup(createDto, cancellationToken);
    }

    /// <summary>
    ///     Removes a member from a user group
    /// </summary>
    [HttpDelete("leave")]
    public async Task LeaveGroup([FromQuery] Guid groupId, [FromQuery] Guid userId, CancellationToken cancellationToken)
    {
        await memberService.LeaveGroup(groupId, userId, cancellationToken);
    }

    /// <summary>
    ///     Promotes a group member to moderator
    /// </summary>
    [HttpPatch("members/{memberId:guid}/role")]
    public async Task<GroupMemberViewModel> ChangeRole(Guid memberId, CancellationToken cancellationToken)
    {
        var newRole = await memberService.ChangeRole(memberId, cancellationToken);
        return mapper.Map<GroupMemberViewModel>(newRole);
    }

    /// <summary>
    ///     Shares a subscription with a user group
    /// </summary>
    [HttpPost("share")]
    public async Task<UserGroupViewModel> ShareSubscription([FromQuery] Guid groupId, [FromQuery] Guid subscriptionId,
        CancellationToken cancellationToken)
    {
        var updatedGroup = await service.ShareSubscription(groupId, subscriptionId, cancellationToken);
        return mapper.Map<UserGroupViewModel>(updatedGroup);
    }

    /// <summary>
    ///     Removes a shared subscription from a user group
    /// </summary>
    [HttpPost("unshare")]
    public async Task<UserGroupViewModel> UnshareSubscription([FromQuery] Guid groupId, [FromQuery] Guid subscriptionId,
        CancellationToken cancellationToken)
    {
        var updatedGroup = await service.UnshareSubscription(groupId, subscriptionId, cancellationToken);
        return mapper.Map<UserGroupViewModel>(updatedGroup);
    }
}
