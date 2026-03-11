using Allure.Net.Commons;
using Allure.Xunit.Attributes;
using AutoFixture;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Polly.CircuitBreaker;
using Polly.Registry;
using Shouldly;
using SubsTracker.API.Auth0;
using SubsTracker.API.Helpers;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Interfaces;
using SubsTracker.IntegrationTests.Configuration;
using SubsTracker.IntegrationTests.Helpers;

namespace SubsTracker.IntegrationTests.Orchestrator;

public class OrchestratorResilienceTests : IClassFixture<TestsWebApplicationFactory>
{
    private readonly TestsWebApplicationFactory _factory;

    public OrchestratorResilienceTests(TestsWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    [AllureFeature("Resilience")]
    [AllureStory("User Update Orchestration")]
    public async Task FullUserUpdate_ShouldRetry_WhenAuth0ServiceFails()
    {
        // Arrange
        var fixture = new Fixture();
        var updateDto = fixture.Create<UpdateUserDto>();
        var userId = fixture.Create<string>();
        var mockAuth0 = Substitute.For<IAuth0Service>();
        var mockUser = Substitute.For<IUserService>();

        var attempts = 0;
        mockAuth0.UpdateUserProfile(Arg.Any<string>(), Arg.Any<UpdateUserDto>(), Arg.Any<CancellationToken>())
            .Returns(async _ => 
            {
                attempts++;
                if (attempts == 1) throw new Exception("Transient Auth0 Error");
                await Task.CompletedTask;
            });

        using var scope = _factory.Services.CreateScope();
        var pipelineProvider = scope.ServiceProvider.GetRequiredService<ResiliencePipelineProvider<string>>();
        var orchestrator = new UserUpdateOrchestrator(mockAuth0, mockUser, pipelineProvider);

        // Act
        await AllureApi.Step("Execute Full User Update with Transient Failure", async () =>
        {
            await orchestrator.FullUserUpdate(userId, updateDto, CancellationToken.None);
        });

        // Assert
        AllureApi.Step("Verify Polly Retry Logic", () =>
        {
            attempts.ShouldBe(2); 
            mockAuth0.Received(2).UpdateUserProfile(userId, updateDto, Arg.Any<CancellationToken>());
        });
    }
    
    [Fact]
    [AllureFeature("Resilience")]
    [AllureStory("Circuit Breaker Protection")]
    public async Task FullUserUpdate_ShouldOpenCircuit_AfterRepeatedFailures()
    {
        // Arrange
        var fixture = new Fixture();
        var auth0Service = Substitute.For<IAuth0Service>();
        var userService = Substitute.For<IUserService>();
        var validDto = new UpdateUserDto 
        { 
            FirstName = "John", 
            LastName = "Doe" 
        };

        auth0Service.UpdateUserProfile(Arg.Any<string>(), validDto, Arg.Any<CancellationToken>())
            .Throws(new TimeoutException("Auth0 is hanging"));

        using var scope = _factory.Services.CreateScope();
        var pipelineProvider = scope.ServiceProvider.GetRequiredService<ResiliencePipelineProvider<string>>();
        var orchestrator = new UserUpdateOrchestrator(auth0Service, userService, pipelineProvider);

        // Act & Assert
        await AllureApi.Step("Exhaust the MinimumThroughput (2 failures)", async () =>
        {
            await Should.ThrowAsync<TimeoutException>(async () => 
                await orchestrator.FullUserUpdate(TestsAuthHandler.DefaultAuth0Id, validDto, CancellationToken.None));
        });
        await Task.Yield();

        await AllureApi.Step("Subsequent call: Verify Circuit is OPEN", async () =>
        {
            await Should.ThrowAsync<BrokenCircuitException>(async () => 
                await orchestrator.FullUserUpdate(TestsAuthHandler.DefaultAuth0Id, validDto, CancellationToken.None));
        });
    }
}
