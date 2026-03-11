using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.BLL.Services.Cache;

namespace SubsTracker.BLL.DI;

public static class CacheDependencies
{
    public static IServiceCollection AddCacheDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddStackExchangeRedisCache(redisOptions =>
            {
                redisOptions.Configuration = configuration["Redis"] ?? "localhost:6379,abortConnect=false";
                redisOptions.InstanceName = "Redis_";
            })
            .AddSingleton<IConnectionMultiplexer>(_ =>
            {
                var redisConnection = configuration["Redis"] ?? "localhost:6379,abortConnect=false";
                var options = ConfigurationOptions.Parse(redisConnection);
                options.AbortOnConnectFail = false;
                return ConnectionMultiplexer.Connect(options);
            })
            .AddSingleton<IDistributedLockFactory>(serviceProvider =>
                RedLockFactory.Create(new List<RedLockMultiplexer>
                {
                    new(serviceProvider.GetRequiredService<IConnectionMultiplexer>())
                }))
            .AddScoped<ICacheService, CacheService>()
            .AddScoped<ICacheAccessService, CacheAccessService>();

        return services;
    }
}
