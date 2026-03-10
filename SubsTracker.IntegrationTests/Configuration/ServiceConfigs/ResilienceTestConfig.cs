using Microsoft.Extensions.Configuration;

namespace SubsTracker.IntegrationTests.Configuration.ServiceConfigs;

public static class ResilienceTestConfig
{
    public static void AddFakeResilienceConfig(this IConfigurationBuilder config)
    {
        var testResilienceConfig = new Dictionary<string, string?>
        {
            ["Retry:MaxRetryAttempts"] = "1",
            ["Retry:BaseDelaySeconds"] = "0.1",
            ["Retry:SecondsTimeout"] = "5.0",
            
            ["CapacityLimiter:PermitLimit"] = "2000",
            ["CapacityLimiter:RequestWindow"] = "60",
            ["CapacityLimiter:SegmentsPerWindow"] = "10",
            
            ["CircuitBreaker:FailureRatio"] = "0.5", 
            ["CircuitBreaker:SamplingDuration"] = "30",
            ["CircuitBreaker:MinimumThroughput"] = "2",
            ["CircuitBreaker:BreakDuration"] = "5"
        };
        config.AddInMemoryCollection(testResilienceConfig);
    }
}
