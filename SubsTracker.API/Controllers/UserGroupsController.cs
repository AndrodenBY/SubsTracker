using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    /// <param name="id">The ID of the user group</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <remarks>
    ///     Sample request:
    ///     GET /api/usergroups/{id}
    /// </remarks>
    /// <returns>The user group view model</returns>
    [HttpGet("{id:guid}")]
    public async Task<UserGroupViewModel> GetById(Guid id, CancellationToken cancellationToken)
    {
        var getById = await service.GetFullInfoById(id, cancellationToken);
        return mapper.Map<UserGroupViewModel>(getById);
    }

    /// <summary>
    ///     Retrieves all user groups with optional filtering
    /// </summary>
    /// <param name="filterDto">Filter parameters for the user groups</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <remarks>
    ///     Sample request:
    ///     GET /api/usergroups?{name}
    /// </remarks>
    /// <returns>A list of user group view models</returns>
    [HttpGet]
    public async Task<List<UserGroupViewModel>> GetAll([FromQuery] UserGroupFilterDto? filterDto,
        CancellationToken cancellationToken)
    {
        var getAll = await service.GetAll(filterDto, cancellationToken);
        return mapper.Map<List<UserGroupViewModel>>(getAll);
    }

    /// <summary>
    ///     Retrieves all members of a group with optional filtering
    /// </summary>
    /// <param name="filterDto">Filter parameters for group members</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <remarks>
    ///     Sample request:
    ///     GET /api/usergroups/members?GroupId={groupId}
    /// </remarks>
    /// <returns>A list of group member view models</returns>
    [HttpGet("members")]
    public async Task<List<GroupMemberViewModel>> GetAllMembers([FromQuery] GroupMemberFilterDto? filterDto,
        CancellationToken cancellationToken)
    {
        var entities = await memberService.GetAll(filterDto, cancellationToken);
        return mapper.Map<List<GroupMemberViewModel>>(entities);
    }

    /// <summary>
    ///     Creates a new user group
    /// </summary>
    /// <param name="userId">The ID of the user, who creates the group</param>
    /// <param name="createDto">The user group data</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <remarks>
    ///     Sample request:
    ///     POST /api/usergroups/{userId}/create
    ///     {
    ///     "name": "Team A"
    ///     }
    /// </remarks>
    /// <returns>The created user group view model</returns>
    /// <exception cref="Domain.Exceptions.ValidationException">Thrown if cannot find the user with that ID</exception>
    [HttpPost("{userId:guid}/create")]
    public async Task<UserGroupViewModel> Create(Guid userId, CreateUserGroupDto createDto,
        CancellationToken cancellationToken)
    {
        var create = await service.Create(userId, createDto, cancellationToken);
        return mapper.Map<UserGroupViewModel>(create);
    }

    /// <summary>
    ///     Updates an existing user group
    /// </summary>
    /// <param name="id">The ID of the user group to update</param>
    /// <param name="updateDto">The updated user group data</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <remarks>
    ///     Sample request:
    ///     PUT /api/usergroups/{id}
    ///     {
    ///     "name": "Team B"
    ///     }
    /// </remarks>
    /// <returns>The updated user group view model</returns>
    /// <exception cref="Domain.Exceptions.NotFoundException">Thrown if cannot find the UserGroup with that ID</exception>
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
    /// <param name="id">The ID of the user group to delete</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <remarks>
    ///     Sample request:
    ///     DELETE /api/usergroups/{id}
    /// </remarks>
    /// <returns>No content if successful</returns>
    /// <exception cref="Domain.Exceptions.NotFoundException">Thrown if cannot find the UserGroup with that ID</exception>
    [HttpDelete("{id:guid}")]
    public async Task Delete(Guid id, CancellationToken cancellationToken)
    {
        await service.Delete(id, cancellationToken);
    }

    /// <summary>
    ///     Adds a user to a group
    /// </summary>
    /// <param name="createDto">The group member data</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <remarks>
    ///     Sample request:
    ///     POST /api/usergroups/join
    ///     {
    ///     "userId": {userId},
    ///     "groupId": {groupId}
    ///     }
    /// </remarks>
    /// <returns>The created group member view model</returns>
    /// <exception cref="Domain.Exceptions.ValidationException">Thrown if member already exists in this group</exception>
    [HttpPost("join")]
    public async Task JoinGroup([FromBody] CreateGroupMemberDto createDto, CancellationToken cancellationToken)
    {
        await memberService.JoinGroup(createDto, cancellationToken);
    }

    /// <summary>
    ///     Removes a member from a user group
    /// </summary>
    /// <param name="groupId">The ID of the group</param>
    /// <param name="userId">The ID of the user to remove</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <remarks>
    ///     Sample request:
    ///     DELETE /api/usergroups/leave?groupId={groupId}6&userId={userId}
    /// </remarks>
    /// <returns>No content if successful</returns>
    /// <exception cref="Domain.Exceptions.NotFoundException">Thrown if member is not found in the group</exception>
    [HttpDelete("leave")]
    public async Task LeaveGroup([FromQuery] Guid groupId, [FromQuery] Guid userId, CancellationToken cancellationToken)
    {
        await memberService.LeaveGroup(groupId, userId, cancellationToken);
    }

    /// <summary>
    ///     Promotes a group member to moderator
    /// </summary>
    /// <param name="memberId">The ID of the group member to promote</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <remarks>
    ///     Sample request:
    ///     PUT /api/usergroups/members/{memberId}/role
    /// </remarks>
    /// <returns>The updated group member view model</returns>
    /// <exception cref="Domain.Exceptions.NotFoundException">Thrown if member is not found</exception>
    [HttpPatch("members/{memberId:guid}/role")]
    public async Task<GroupMemberViewModel> ChangeRole(Guid memberId, CancellationToken cancellationToken)
    {
        var newRole = await memberService.ChangeRole(memberId, cancellationToken);
        return mapper.Map<GroupMemberViewModel>(newRole);
    }

    /// <summary>
    ///     Shares a subscription with a user group
    /// </summary>
    /// <param name="groupId">The ID of the group</param>
    /// <param name="subscriptionId">The ID of the subscription to share</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <remarks>
    ///     Sample request:
    ///     POST /api/usergroups/share?groupId={groupId}&subscriptionId={subscriptionId}
    /// </remarks>
    /// <returns>The updated user group view model</returns>
    /// <exception cref="Domain.Exceptions.NotFoundException">Thrown if group or subscription is not found</exception>
    /// <exception cref="InvalidOperationException">Thrown if subscription is already shared</exception>
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
    /// <param name="groupId">The ID of the group</param>
    /// <param name="subscriptionId">The ID of the subscription to unshare</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <remarks>
    ///     Sample request:
    ///     POST /api/usergroups/unshare?groupId={groupId}&subscriptionId={subscriptionId}
    /// </remarks>
    /// <returns>The updated user group view model</returns>
    /// <exception cref="Domain.Exceptions.NotFoundException">Thrown if subscription is not found</exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when the subscription is not associated with the group during an exclusion
    ///     attempt
    /// </exception>
    [HttpPost("unshare")]
    public async Task<UserGroupViewModel> UnshareSubscription([FromQuery] Guid groupId, [FromQuery] Guid subscriptionId,
        CancellationToken cancellationToken)
    {
        var updatedGroup = await service.UnshareSubscription(groupId, subscriptionId, cancellationToken);
        return mapper.Map<UserGroupViewModel>(updatedGroup);
    }
}