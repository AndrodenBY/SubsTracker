using Microsoft.Extensions.DependencyInjection.Extensions;
using SubsTracker.BLL.Interfaces.Cache;

namespace SubsTracker.IntegrationTests.Configuration.WebApplicationFactory;

public static class CacheTestConfig
{
    public static IServiceCollection ReplaceCache(this IServiceCollection services)
    {
        services.RemoveAll<ICacheService>();
        services.AddSingleton<ICacheService, FakeCacheService>();

        services.RemoveAll<ICacheAccessService>();
        services.AddSingleton<ICacheAccessService, FakeCacheAccessService>();

        return services;
    }

    public static IServiceCollection ReplaceCache(this IServiceCollection services, ICacheService cacheService)
    {
        services.RemoveAll<ICacheService>();
        services.AddSingleton(cacheService);
        return services;
    }

    public static IServiceCollection ReplaceCacheAccess(this IServiceCollection services, ICacheAccessService accessService)
    {
        services.RemoveAll<ICacheAccessService>();
        services.AddSingleton(accessService);
        return services;
    }
}

