using SubsTracker.BLL.DTOs.Filter;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using UserModel = SubsTracker.DAL.Models.User.User;

namespace SubsTracker.BLL.Interfaces.User;

public interface IUserService : IService<UserModel, UserDto, CreateUserDto, UpdateUserDto, UserFilterDto>
{
    Task<IEnumerable<UserDto>> GetAll(UserFilterDto? filter, CancellationToken cancellationToken);
    Task<UserDto?> GetByEmail(string email, CancellationToken cancellationToken);
}
