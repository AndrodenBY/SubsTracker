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

    public async Task<string> GetClientCredentialsToken(CancellationToken cancellationToken)
    {
        var token = await authClient.GetTokenAsync(
            new ClientCredentialsTokenRequest
            {
                ClientId = _options.ClientId,
                ClientSecret = _options.ClientSecret,
                Audience = _options.Audience
            },
            cancellationToken
        );

        return token.AccessToken;
    }
    
    public async Task UpdateUserProfile(string userId, UpdateUserDto dto, CancellationToken cancellationToken)
    {
        var token = await GetClientCredentialsToken(cancellationToken);

        var mgmt = new ManagementApiClient(token, _options.ManagementApiUrl);
        await mgmt.Users.UpdateAsync(userId, new UserUpdateRequest
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email
        }, cancellationToken);
    }
}
