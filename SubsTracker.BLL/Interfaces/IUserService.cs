using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.DAL.Entities;
using SubsTracker.Domain.Filter;

namespace SubsTracker.BLL.Interfaces;

public interface IUserService : IService<UserEntity, UserDto, CreateUserDto, UpdateUserDto, UserFilterDto>
{
    Task<List<UserDto>> GetAll(UserFilterDto? filter, CancellationToken cancellationToken);
    Task<UserDto?> GetByAuth0Id(string auth0Id, CancellationToken cancellationToken);
    Task<UserDto> Create(string auth0Id, CreateUserDto createDto, CancellationToken cancellationToken);
    Task<UserDto> Update(string auth0Id, UpdateUserDto updateDto, CancellationToken cancellationToken);
    Task<bool> Delete(string auth0Id, CancellationToken cancellationToken);
}
