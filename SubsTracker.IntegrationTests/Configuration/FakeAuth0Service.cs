using SubsTracker.API.Auth0;

namespace SubsTracker.IntegrationTests.Configuration;

public class FakeAuth0Service : IAuth0Service
{
    public Task<string> GetClientCredentialsToken(CancellationToken cancellationToken)
    {
        return Task.FromResult("fake-ci-token-12345");
    }

    public Task UpdateUserProfile(string auth0Id, UpdateUserDto updateDto, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
