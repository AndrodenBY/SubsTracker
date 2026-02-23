using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubsTracker.API.Extension;
using SubsTracker.API.ViewModel;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Interfaces;
using SubsTracker.Domain.Filter;

namespace SubsTracker.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class GroupsController(
    IGroupService service,
    IMemberService memberService,
    IMapper mapper) 
    : ControllerBase
{
    /// <summary>
    ///     Retrieves a user group by its ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<GroupViewModel> GetById(Guid id, CancellationToken cancellationToken)
    {
        var getById = await service.GetFullInfoById(id, cancellationToken);
        return mapper.Map<GroupViewModel>(getById);
    }

    /// <summary>
    ///     Retrieves all user groups with optional filtering
    /// </summary>
    [HttpGet]
    public async Task<List<GroupViewModel>> GetAll([FromQuery] GroupFilterDto? filterDto, CancellationToken cancellationToken)
    {
        var getAll = await service.GetAll(filterDto, cancellationToken);
        return mapper.Map<List<GroupViewModel>>(getAll);
    }

    /// <summary>
    ///     Retrieves all members of a group with optional filtering
    /// </summary>
    [HttpGet("members")]
    public async Task<List<MemberViewModel>> GetAllMembers([FromQuery] MemberFilterDto? filterDto, CancellationToken cancellationToken)
    {
        var entities = await memberService.GetAll(filterDto, cancellationToken);
        return mapper.Map<List<MemberViewModel>>(entities);
    }

    /// <summary>
    ///     Creates a new user group
    /// </summary>
    [HttpPost]
    public async Task<GroupViewModel> Create(CreateGroupDto createDto, CancellationToken cancellationToken)
    {
        var create = await service.Create(User.GetAuth0IdFromToken(), createDto, cancellationToken);
        return mapper.Map<GroupViewModel>(create);
    }

    /// <summary>
    ///     Updates an existing user group
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<GroupViewModel> Update(Guid id, [FromBody] UpdateGroupDto updateDto, CancellationToken cancellationToken)
    {
        var update = await service.Update(id, updateDto, cancellationToken);
        return mapper.Map<GroupViewModel>(update);
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
    public async Task JoinGroup([FromBody] CreateMemberDto createDto, CancellationToken cancellationToken)
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
    public async Task<MemberViewModel> ChangeRole(Guid memberId, CancellationToken cancellationToken)
    {
        var newRole = await memberService.ChangeRole(memberId, cancellationToken);
        return mapper.Map<MemberViewModel>(newRole);
    }

    /// <summary>
    ///     Shares a subscription with a user group
    /// </summary>
    [HttpPost("share")]
    public async Task<GroupViewModel> ShareSubscription([FromQuery] Guid groupId, [FromQuery] Guid subscriptionId, CancellationToken cancellationToken)
    {
        var updatedGroup = await service.ShareSubscription(groupId, subscriptionId, cancellationToken);
        return mapper.Map<GroupViewModel>(updatedGroup);
    }

    /// <summary>
    ///     Removes a shared subscription from a user group
    /// </summary>
    [HttpPost("unshare")]
    public async Task<GroupViewModel> UnshareSubscription([FromQuery] Guid groupId, [FromQuery] Guid subscriptionId, CancellationToken cancellationToken)
    {
        var updatedGroup = await service.UnshareSubscription(groupId, subscriptionId, cancellationToken);
        return mapper.Map<GroupViewModel>(updatedGroup);
    }
}
