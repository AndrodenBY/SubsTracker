using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.Domain.Filter;
using UserModel = SubsTracker.DAL.Models.User.User;

namespace SubsTracker.BLL.Interfaces.User;

public interface IUserService : IService<UserModel, UserDto, CreateUserDto, UpdateUserDto, UserFilterDto>
{
    Task<List<UserDto>> GetAll(UserFilterDto? filter, CancellationToken cancellationToken);
    Task<UserDto?> GetByAuth0Id(string auth0Id, CancellationToken cancellationToken);
}
