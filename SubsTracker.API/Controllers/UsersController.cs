using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubsTracker.API.Extension;
using SubsTracker.API.Helpers;
using SubsTracker.API.ViewModel;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Filter;
using SubsTracker.BLL.Interfaces;
using SubsTracker.Domain.Pagination;

namespace SubsTracker.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController(
    IUserService service,
    IMapper mapper) 
    : ControllerBase
{
    /// <summary>
    ///     Retrieves a user by their ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<UserViewModel> GetById(Guid id, CancellationToken cancellationToken)
    {
        var getById = await service.GetById(id, cancellationToken);
        return mapper.Map<UserViewModel>(getById);
    }

    /// <summary>
    ///     Retrieves the profile of the currently authenticated user.
    /// </summary>
    [HttpGet("me")]
    public async Task<UserViewModel> GetByIdentityId(CancellationToken cancellationToken)
    {
        var user = await service.GetByIdentityId(User.GetIdentityIdFromToken(), cancellationToken);
        return mapper.Map<UserViewModel>(user);
    }

    /// <summary>
    ///     Retrieves all users with optional filtering.
    /// </summary>
    [HttpGet]
    public async Task<PaginatedList<UserViewModel>> GetAll([FromQuery] UserFilterDto? filterDto, [FromQuery] PaginationParameters? paginationParameters, CancellationToken cancellationToken)
    {
        var pagedResult = await service.GetAll(filterDto, paginationParameters, cancellationToken);
        return pagedResult.MapToPage(mapper.Map<UserViewModel>);
    }

    /// <summary>
    ///     Creates a new user.
    /// </summary>
    [HttpPost]
    public async Task<UserViewModel> Create([FromBody] CreateUserDto createDto, CancellationToken cancellationToken)
    {
        var create = await service.Create(User.GetIdentityIdFromToken(), createDto, cancellationToken);
        return mapper.Map<UserViewModel>(create);
    }

    /// <summary>
    ///     Updates an existing user both in DB and in Auth0
    /// </summary>
    [HttpPut("me")]
    public async Task<UserViewModel> Update([FromBody] UpdateUserDto updateDto, [FromServices] UserUpdateOrchestrator updateOrchestrator, CancellationToken cancellationToken)
    {
        var identityId =  User.GetIdentityIdFromToken();
        var updatedUser = await updateOrchestrator.FullUserUpdate(identityId, updateDto, cancellationToken);
    
        return mapper.Map<UserViewModel>(updatedUser);
    }

    /// <summary>
    ///     Deletes a user by their Auth0 ID
    /// </summary>
    [HttpDelete]
    public async Task Delete(CancellationToken cancellationToken)
    {
        await service.Delete(User.GetIdentityIdFromToken(), cancellationToken);
    }
}
