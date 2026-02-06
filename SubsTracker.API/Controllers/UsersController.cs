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
public class UsersController(
    IUserService service,
    IMapper mapper
) : ControllerBase
{
    /// <summary>
    ///     Retrieves a user by their ID.
    /// </summary>
    /// <param name="id">The ID of the user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <remarks>
    ///     Sample request:
    ///     GET /api/users/{id}
    /// </remarks>
    /// <returns>The user view model.</returns>
    [HttpGet("{id:guid}")]
    public async Task<UserViewModel> GetById(Guid id, CancellationToken cancellationToken)
    {
        var getById = await service.GetById(id, cancellationToken);
        return mapper.Map<UserViewModel>(getById);
    }

    /// <summary>
    ///     Retrieves all users with optional filtering.
    /// </summary>
    /// <param name="filterDto">Filter parameters for the users.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <remarks>
    ///     Sample request:
    ///     GET /api/users?email={email}
    /// </remarks>
    /// <returns>A list of user view models.</returns>
    [HttpGet]
    public async Task<List<UserViewModel>> GetAll([FromQuery] UserFilterDto? filterDto,
        CancellationToken cancellationToken)
    {
        var getAll = await service.GetAll(filterDto, cancellationToken);
        return mapper.Map<List<UserViewModel>>(getAll);
    }

    /// <summary>
    ///     Creates a new user.
    /// </summary>
    /// <param name="createDto">The user data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <remarks>
    ///     Sample request:
    ///     POST /api/users
    ///     {
    ///     "firstName": "John",
    ///     "lastName": "Doe",
    ///     "email": "john.doe@example.com"
    ///     }
    /// </remarks>
    /// <returns>The created user view model.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the user with that email already exists</exception>
    [HttpPost]
    public async Task<UserViewModel> Create([FromBody] CreateUserDto createDto, CancellationToken cancellationToken)
    {
        var create = await service.Create(createDto, cancellationToken);
        return mapper.Map<UserViewModel>(create);
    }

    /// <summary>
    ///     Updates an existing user.
    /// </summary>
    /// <param name="id">The ID of the user to update.</param>
    /// <param name="updateDto">The updated user data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <remarks>
    ///     Sample request:
    ///     PUT /api/users/{id}
    ///     {
    ///     "firstName": "John"
    ///     }
    /// </remarks>
    /// <returns>The updated user view model.</returns>
    /// <exception cref="InvalidOperationException">Thrown if cannot find the user with that ID</exception>
    [HttpPut("{id:guid}")]
    public async Task<UserViewModel> Update(Guid id, [FromBody] UpdateUserDto updateDto,
        CancellationToken cancellationToken)
    {
        var update = await service.Update(id, updateDto, cancellationToken);
        return mapper.Map<UserViewModel>(update);
    }

    /// <summary>
    ///     Deletes a user by their ID
    /// </summary>
    /// <param name="id">The ID of the user to delete</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <remarks>
    ///     Sample request:
    ///     DELETE /api/users/{id}
    /// </remarks>
    /// <returns>No content if successful</returns>
    /// <exception cref="Domain.Exceptions.NotFoundException">Thrown if the user not found</exception>
    [HttpDelete("{id:guid}")]
    public async Task Delete(Guid id, CancellationToken cancellationToken)
    {
        await service.Delete(id, cancellationToken);
    }
}

