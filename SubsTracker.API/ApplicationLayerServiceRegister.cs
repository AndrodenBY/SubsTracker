using System.Data.Common;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Auth0.AuthenticationApi;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.CircuitBreaker;
using Polly.RateLimiting;
using Polly.Retry;
using SubsTracker.API.Auth0;
using SubsTracker.API.Constants;
using SubsTracker.API.Helpers;
using SubsTracker.API.Mapper;
using SubsTracker.API.Resilience;
using SubsTracker.API.Validators.User;
using SubsTracker.BLL;

namespace SubsTracker.API;

public static class ApplicationLayerServiceRegister
{
    public static void RegisterApplicationLayerDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterBusinessLayerDependencies(configuration)
            .AddAutoMapper(_ => { }, typeof(ViewModelMappingProfile).Assembly)
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });
        
        services.AddFluentValidationAutoValidation()
            .AddValidatorsFromAssemblyContaining<CreateUserDtoValidator>();

        services.AddCors(options =>
            options.AddDefaultPolicy(policy =>
                policy.WithOrigins("http://localhost:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
            ));
        
        services.AddOptions<RetryOptions>()
            .BindConfiguration(RetryOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddOptions<CircuitBreakerOptions>()
            .BindConfiguration(CircuitBreakerOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddOptions<CapacityLimiterOptions>()
            .BindConfiguration(CapacityLimiterOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddResiliencePipeline(ResilienceConstants.OrchestratorPipeline, (builder, context) => 
        {
            var retryOptions = context.ServiceProvider.GetRequiredService<IOptions<RetryOptions>>().Value;
            var circuitBreakerOptions = context.ServiceProvider.GetRequiredService<IOptions<CircuitBreakerOptions>>().Value;
            var capacityLimiterOptions = context.ServiceProvider.GetRequiredService<IOptions<CapacityLimiterOptions>>().Value;
            
            builder.AddRateLimiter(new SlidingWindowRateLimiter(
                new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = capacityLimiterOptions.PermitLimit,
                    Window = TimeSpan.FromSeconds(capacityLimiterOptions.RequestWindow),
                    SegmentsPerWindow = capacityLimiterOptions.SegmentsPerWindow
                }));
    
            builder.AddRateLimiter(new SlidingWindowRateLimiter(
                new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = capacityLimiterOptions.PermitLimit,
                    Window = TimeSpan.FromSeconds(capacityLimiterOptions.RequestWindow),
                    SegmentsPerWindow = capacityLimiterOptions.SegmentsPerWindow
                }));
            
            builder.AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = retryOptions.MaxRetryAttempts,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                Delay = TimeSpan.FromSeconds(retryOptions.BaseDelaySeconds)
            });
            
            builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = circuitBreakerOptions.FailureRatio,
                SamplingDuration = TimeSpan.FromSeconds(circuitBreakerOptions.SamplingDuration),
                MinimumThroughput = circuitBreakerOptions.MinimumThroughput,
                BreakDuration = TimeSpan.FromSeconds(circuitBreakerOptions.BreakDuration),
                ShouldHandle = new PredicateBuilder()
                    .Handle<DbException>()
                    .Handle<SqlException>()
                    .Handle<TimeoutException>()
            });

            builder.AddTimeout(TimeSpan.FromSeconds(retryOptions.SecondsTimeout));
        });
        
        services.AddOptions<Auth0Options>()
            .BindConfiguration(Auth0Options.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart(); 
        
        services.AddSingleton<AuthenticationApiClient>(serviceProvider => 
        {
            var options = serviceProvider.GetRequiredService<IOptions<Auth0Options>>().Value;
            return new AuthenticationApiClient(new Uri(options.Authority));
        });
    
        services.AddScoped<UserUpdateOrchestrator>()
            .AddScoped<IAuth0Service, Auth0Service>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<Auth0Options>>((options, auth0) =>
            {
                options.Authority = auth0.Value.Authority;
                options.Audience = auth0.Value.Audience;
        
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = ClaimTypes.NameIdentifier,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                };
            });
    }
}
