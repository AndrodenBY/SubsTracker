using SubsTracker.API.Auth0;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Interfaces.User;

namespace SubsTracker.API.Helpers;

public class UserUpdateOrchestrator(IAuth0Service auth0Service, IUserService userService)
{
    public async Task<UserDto> FullUserUpdate(string auth0Id, UpdateUserDto updateDto, CancellationToken cancellationToken)
    {
        await auth0Service.UpdateUserProfile(auth0Id, updateDto, cancellationToken);

        return await userService.Update(auth0Id, updateDto, cancellationToken);
    }
}
