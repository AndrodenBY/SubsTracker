using System.Data.Common;
using System.Threading.RateLimiting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using SubsTracker.API.Extension;
using SubsTracker.API.Resilience;

namespace SubsTracker.API.DI;

public static class ResilienceDependencies
{
    public static IServiceCollection AddResilienceDependencies(this IServiceCollection services)
    {
        services.RegisterOptions<RetryOptions>(RetryOptions.SectionName);
        services.RegisterOptions<CircuitBreakerOptions>(CircuitBreakerOptions.SectionName);
        services.RegisterOptions<CapacityLimiterOptions>(CapacityLimiterOptions.SectionName);
        
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
        
        return services;
    }
}
