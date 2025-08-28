using AutoMapper;
using SubsTracker.BLL.Interfaces;
using SubsTracker.Domain.Interfaces;
using UserDto = SubsTracker.BLL.DTOs.User.UserDto;
using CreateUserDto = SubsTracker.BLL.DTOs.User.Create.CreateUserDto;
using UpdateUserDto = SubsTracker.BLL.DTOs.User.Update.UpdateUserDto;
using UserModel = SubsTracker.DAL.Models.User.User;

namespace SubsTracker.BLL.Services.User;

public class UserService(IRepository<UserModel> repository, IMapper mapper) 
    : ServiceBase<UserModel, UserDto, CreateUserDto, UpdateUserDto>(repository, mapper), IUserService
{
    public async Task<UserDto?> GetByEmail(string email, CancellationToken cancellationToken)
    {
        var user = await repository.GetByPredicate(u => u.Email == email, cancellationToken);
        return mapper.Map<UserDto>(user);
    }
}



