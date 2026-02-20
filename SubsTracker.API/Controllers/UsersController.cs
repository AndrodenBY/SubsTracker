using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubsTracker.API.Extension;
using SubsTracker.API.Helpers;
using SubsTracker.API.ViewModel;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Interfaces;
using SubsTracker.Domain.Filter;

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
    public async Task<UserViewModel> GetByAuth0Id(CancellationToken cancellationToken)
    {
        var user = await service.GetByAuth0Id(User.GetAuth0IdFromToken(), cancellationToken);
        return mapper.Map<UserViewModel>(user);
    }

    /// <summary>
    ///     Retrieves all users with optional filtering.
    /// </summary>
    [HttpGet]
    public async Task<List<UserViewModel>> GetAll([FromQuery] UserFilterDto? filterDto, CancellationToken cancellationToken)
    {
        var getAll = await service.GetAll(filterDto, cancellationToken);
        return mapper.Map<List<UserViewModel>>(getAll);
    }

    /// <summary>
    ///     Creates a new user.
    /// </summary>
    [HttpPost]
    public async Task<UserViewModel> Create([FromBody] CreateUserDto createDto, CancellationToken cancellationToken)
    {
        
        var create = await service.Create(User.GetAuth0IdFromToken(), createDto, cancellationToken);
        return mapper.Map<UserViewModel>(create);
    }

    /// <summary>
    ///     Updates an existing user both in DB and in Auth0
    /// </summary>
    [HttpPut("me")]
    public async Task<UserViewModel> Update([FromBody] UpdateUserDto updateDto, [FromServices] UserUpdateOrchestrator updateOrchestrator, CancellationToken cancellationToken)
    {
        var auth0Id =  User.GetAuth0IdFromToken();
        var updatedUser = await updateOrchestrator.FullUserUpdate(auth0Id, updateDto, cancellationToken);
    
        return mapper.Map<UserViewModel>(updatedUser);
    }

    /// <summary>
    ///     Deletes a user by their Auth0 ID
    /// </summary>
    [HttpDelete]
    public async Task Delete(CancellationToken cancellationToken)
    {
        await service.Delete(User.GetAuth0IdFromToken(), cancellationToken);
    }
}
