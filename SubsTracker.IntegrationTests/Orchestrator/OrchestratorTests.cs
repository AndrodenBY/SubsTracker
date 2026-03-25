using System.Security.Claims;
using Allure.Net.Commons;
using Allure.Xunit.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Polly;
using Polly.CircuitBreaker;
using Polly.Registry;
using Polly.Retry;
using Shouldly;
using SubsTracker.API.Auth.IdentityProvider;
using SubsTracker.API.Helpers;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Interfaces;
using SubsTracker.IntegrationTests.Configuration;
using SubsTracker.IntegrationTests.Helpers;

namespace SubsTracker.IntegrationTests.Orchestrator;

//[Collection("ResilienceTestCollection")]
[AllureSuite("Integration Tests")]
[AllureFeature("Resilience")]
public class OrchestratorTests : IClassFixture<TestsWebApplicationFactory>
{
    private readonly TestsWebApplicationFactory _factory;
    private readonly HttpContext _mockContext;
    private readonly IUserService _userService;
    private readonly ResiliencePipelineProvider<string> _pipelineProvider;
    private readonly UserGetOrchestrator _getOrchestrator;

    public OrchestratorTests(TestsWebApplicationFactory factory)
    {
        _factory = factory;
        _mockContext = new DefaultHttpContext();
        _userService = Substitute.For<IUserService>();
        _pipelineProvider = _factory.Services.GetRequiredService<ResiliencePipelineProvider<string>>();
        _getOrchestrator = new UserGetOrchestrator(_userService, _pipelineProvider);
    }
    
    [Fact]
    [AllureFeature("Resilience")]
    [AllureStory("Get Profile Retry Logic")]
    public async Task GetCurrentProfile_ShouldRetry_WhenDatabaseTransientlyFails()
    {
        // Arrange
        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.Zero,
                ShouldHandle = new PredicateBuilder().Handle<Exception>()
            })
            .Build();

        var provider = new TestPipelineProvider(pipeline);

        var getOrchestrator = new UserGetOrchestrator(_userService, provider);
        var internalId = Guid.NewGuid();
        var principal = CreateMockPrincipal(internalId.ToString());
        var attempts = 0;

        _userService.GetById(internalId, Arg.Any<CancellationToken>())
            .Returns( _ => 
            {
                attempts++;
                if (attempts == 1) throw new Exception("Transient DB Error");
                return new UserDto { Id = internalId, IdentityId = TestsAuthHandler.DefaultIdentityId, FirstName = "RandomName"};
            });

        // Act
        var result = await getOrchestrator.GetCurrentProfile(principal, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        attempts.ShouldBe(2);
        await _userService.Received(2).GetById(internalId, Arg.Any<CancellationToken>());
    }

    [Fact]
    [AllureFeature("User Retrieval")]
    [AllureStory("Get Profile by Internal GUID")]
    public async Task GetCurrentProfile_ShouldUseGetById_WhenClaimIsGuid()
    {
        // Arrange
        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.Zero,
                ShouldHandle = new PredicateBuilder().Handle<Exception>()
            })
            .Build();

        var provider = new TestPipelineProvider(pipeline);
        var getOrchestrator = new UserGetOrchestrator(_userService, provider);
        var internalId = Guid.NewGuid();
        var userDto = new UserDto { Id = internalId, IdentityId = TestsAuthHandler.DefaultIdentityId, FirstName = "RandomChel", Email = "test@example.com" };
        var principal = CreateMockPrincipal(internalId.ToString());

        _userService.GetById(internalId, Arg.Any<CancellationToken>())
            .Returns(userDto);

        // Act
        var result = await getOrchestrator.GetCurrentProfile(principal, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(internalId);
        await _userService.Received(1).GetById(internalId, Arg.Any<CancellationToken>());
        await _userService.DidNotReceive().GetByIdentityId(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [AllureFeature("User Retrieval")]
    [AllureStory("Get Profile by Identity ID")]
    public async Task GetCurrentProfile_ShouldUseGetByIdentityId_WhenClaimIsNotGuid()
    {
        // Arrange
        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.Zero,
                ShouldHandle = new PredicateBuilder().Handle<Exception>()
            })
            .Build();

        var provider = new TestPipelineProvider(pipeline);
        var getOrchestrator = new UserGetOrchestrator(_userService, provider);
        var identityId = TestsAuthHandler.DefaultIdentityId;
        var userDto = new UserDto { FirstName = "RandomChel", IdentityId = identityId };
        var principal = CreateMockPrincipal(identityId);

        _userService.GetByIdentityId(identityId, Arg.Any<CancellationToken>())
            .Returns(userDto);

        // Act
        var result = await getOrchestrator.GetCurrentProfile(principal, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IdentityId.ShouldBe(identityId);
        await _userService.Received(1).GetByIdentityId(identityId, Arg.Any<CancellationToken>());
        await _userService.DidNotReceive().GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [AllureFeature("User Retrieval")]
    [AllureStory("Missing Claim Handling")]
    public async Task GetCurrentProfile_ShouldThrowUnauthorized_WhenClaimIsMissing()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        // Act & Assert
        await Should.ThrowAsync<UnauthorizedAccessException>(async () => 
            await _getOrchestrator.GetCurrentProfile(principal, CancellationToken.None));
    }
    
    [Fact]
    [AllureFeature("Resilience")]
    [AllureStory("Circuit Breaker Protection")]
    public async Task FullUserUpdate_ShouldOpenCircuit_AfterRepeatedFailures()
    {
        // Arrange
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
                await orchestrator.FullUserUpdate(_mockContext, Guid.NewGuid(), TestsAuthHandler.DefaultIdentityId, validDto, CancellationToken.None));
        });
        await Task.Yield();

        await AllureApi.Step("Subsequent call: Verify Circuit is OPEN", async () =>
        {
            await Should.ThrowAsync<BrokenCircuitException>(async () => 
                await orchestrator.FullUserUpdate(_mockContext, Guid.NewGuid(), TestsAuthHandler.DefaultIdentityId, validDto, CancellationToken.None));
        });
    }

    [Fact]
    [AllureFeature("Resilience")]
    [AllureStory("Timeout Protection")]
    public async Task FullUserUpdate_ShouldThrowTimeout_WhenServiceHangs()
    {
        var mockAuth0 = Substitute.For<IAuth0Service>();
        var mockUser = Substitute.For<IUserService>();
        
        mockAuth0.UpdateUserProfile(Arg.Any<string>(), Arg.Any<UpdateUserDto>(), Arg.Any<CancellationToken>())
            .Returns(async _ => { await Task.Delay(10000); });

        using var scope = _factory.Services.CreateScope();
        var pipelineProvider = scope.ServiceProvider.GetRequiredService<ResiliencePipelineProvider<string>>();
        var orchestrator = new UserUpdateOrchestrator(mockAuth0, mockUser, pipelineProvider);

        // Act & Assert
        await AllureApi.Step("Verify Timeout cancellation is triggered", async () =>
        {
            
            await Should.ThrowAsync<Exception>(async () => 
                await orchestrator.FullUserUpdate(_mockContext, Guid.NewGuid(), "id", new UpdateUserDto(), CancellationToken.None));
        });
    }

    [Fact]
    [AllureFeature("Resilience")]
    [AllureStory("Atomic Integrity")]
    public async Task FullUserUpdate_ShouldNotUpdateLocalDb_IfAuth0FailsPermanently()
    {
        // Arrange
        var mockAuth0 = Substitute.For<IAuth0Service>();
        var mockUser = Substitute.For<IUserService>();
        
        mockAuth0.UpdateUserProfile(Arg.Any<string>(), Arg.Any<UpdateUserDto>(), Arg.Any<CancellationToken>())
            .Throws(new Exception("Auth0 Fatal Error"));

        using var scope = _factory.Services.CreateScope();
        var pipelineProvider = scope.ServiceProvider.GetRequiredService<ResiliencePipelineProvider<string>>();
        var orchestrator = new UserUpdateOrchestrator(mockAuth0, mockUser, pipelineProvider);

        // Act
        await Should.ThrowAsync<Exception>(async () => 
            await orchestrator.FullUserUpdate(_mockContext, Guid.NewGuid(), "id", new UpdateUserDto(), CancellationToken.None));

        // Assert
        await AllureApi.Step("Verify local database was never touched", async () =>
        {
            await mockUser.DidNotReceiveWithAnyArgs().Update(Guid.Empty, null!, CancellationToken.None);
        });
    }
    
    private static ClaimsPrincipal CreateMockPrincipal(string nameIdentifier)
    {
        var claims = new List<Claim> 
        { 
            new(ClaimTypes.NameIdentifier, nameIdentifier) 
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }
}
