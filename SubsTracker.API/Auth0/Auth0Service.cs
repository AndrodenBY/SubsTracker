using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using Microsoft.Extensions.Options;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.Domain.Options;

namespace SubsTracker.API.Auth0;

public class Auth0Service(AuthenticationApiClient authClient, IOptions<Auth0Options> options) : IAuth0Service
{
    private readonly Auth0Options _options = options.Value;

    public async Task<string> GetClientCredentialsToken(CancellationToken cancellationToken)
    {
        var token = await authClient.GetTokenAsync(
            new ClientCredentialsTokenRequest
            {
                ClientId = _options.ClientId,
                ClientSecret = _options.ClientSecret,
                Audience = _options.ManagementApiUrl
            }, cancellationToken);

        return token.AccessToken;
    }
    
    public async Task UpdateUserProfile(string auth0Id, UpdateUserDto updateDto, CancellationToken cancellationToken)
    {
        var token = await GetClientCredentialsToken(cancellationToken);

        using var managementApi = new ManagementApiClient(token, new Uri(_options.ManagementApiUrl));
        await managementApi.Users.UpdateAsync(auth0Id, new UserUpdateRequest
            {
                FullName = $"{updateDto.FirstName} {updateDto.LastName}",
            }, cancellationToken);
    }
}
