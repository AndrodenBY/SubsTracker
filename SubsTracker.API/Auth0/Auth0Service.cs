using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using Microsoft.Extensions.Options;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.Domain.Options;

namespace SubsTracker.API.Auth0;

public class Auth0Service(AuthenticationApiClient authClient, IOptions<Auth0Options> options)
{
    private readonly Auth0Options _options = options.Value;
    
    public async Task UpdateUserProfile(string auth0Id, UpdateUserDto updateDto, CancellationToken cancellationToken)
    {
        var tokenRequest = new ClientCredentialsTokenRequest
        {
            ClientId = _options.ClientId,
            ClientSecret = _options.ClientSecret,
            Audience = _options.ManagementApiUrl
        };
        
        var tokenResponse = await authClient.GetTokenAsync(tokenRequest, cancellationToken);
        using var managementClient = new ManagementApiClient(tokenResponse.AccessToken, new Uri(_options.ManagementApiUrl));
        
        var updateRequest = new UserUpdateRequest
        {
            FirstName = updateDto.FirstName,
            LastName = updateDto.LastName,
            Email = updateDto.Email,
            FullName = $"{updateDto.LastName} {updateDto.FirstName}"
        };
        
        await managementClient.Users.UpdateAsync(auth0Id, updateRequest, cancellationToken);
    }
}
