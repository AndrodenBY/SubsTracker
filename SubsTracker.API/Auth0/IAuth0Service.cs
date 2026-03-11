using SubsTracker.BLL.DTOs.User.Update;

namespace SubsTracker.API.Auth0;

public interface IAuth0Service
{ 
    Task<string> GetClientCredentialsToken(CancellationToken cancellationToken);
    Task UpdateUserProfile(string identityId, UpdateUserDto updateDto, CancellationToken cancellationToken);
}
