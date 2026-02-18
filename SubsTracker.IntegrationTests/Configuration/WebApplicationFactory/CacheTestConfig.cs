using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace SubsTracker.IntegrationTests.Configuration.WebApplicationFactory;

public static class CacheTestConfig
{
    public static IServiceCollection ConfigureTestContainerCache(this IServiceCollection services, string redisConnectionString)
    {
        var descriptors = services.Where(d => 
            d.ServiceType.Name.Contains("IDistributedCache") ||
            d.ServiceType.Name.Contains("IConnectionMultiplexer")
        ).ToList();

        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
        
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "Tests_";
        });

        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));

        return services;
    }
}
