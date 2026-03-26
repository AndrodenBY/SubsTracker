using SubsTracker.API.Auth.IdentityProvider;
using SubsTracker.BLL.DTOs.User.Update;

namespace SubsTracker.IntegrationTests.Configuration;

public class FakeAuth0Service : IAuth0Service
{
    public Task<string> GetClientCredentialsToken(CancellationToken cancellationToken)
    {
        return Task.FromResult("fake-ci-token-12345");
    }

    public Task UpdateUserProfile(string identityId, UpdateUserDto updateDto, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
