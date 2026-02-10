using Microsoft.Extensions.DependencyInjection.Extensions;
using SubsTracker.API.Auth0;
using SubsTracker.API.Helpers;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.Interfaces.User;

namespace SubsTracker.IntegrationTests.Configuration.WebApplicationFactory;

public static class OrchestratorTestConfig
{
    public static IServiceCollection ReplaceUserUpdateOrchestrator(this IServiceCollection services)
    {
        services.RemoveAll<UserUpdateOrchestrator>();

        var orchestratorMock = Substitute.For<UserUpdateOrchestrator>(
            Substitute.For<IAuth0Service>(),
            Substitute.For<IUserService>()
        );

        orchestratorMock
            .FullUserUpdate(Arg.Any<string>(), Arg.Any<UpdateUserDto>(), Arg.Any<CancellationToken>())
            .Returns(new UserDto { Id = Guid.NewGuid(), FirstName = "Stubbed", Email = "stub@test.com" });

        services.AddScoped(_ => orchestratorMock);

        return services;
    }
}

