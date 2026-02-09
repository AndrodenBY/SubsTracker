using SubsTracker.BLL.DTOs.User.Update;

namespace SubsTracker.API.Auth0;

public interface IAuth0Service
{ 
    Task UpdateUserProfile(string auth0Id, UpdateUserDto updateDto, CancellationToken cancellationToken);
}
