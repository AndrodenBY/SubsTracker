using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.DAL.Models.User;
using SubsTracker.Domain.Interfaces;

namespace SubsTracker.BLL.Interfaces;

public interface IUserService : IService<User, UserDto, CreateUserDto, UpdateUserDto>
{
    Task<UserDto?> GetByEmail(string email, CancellationToken cancellationToken);
}
