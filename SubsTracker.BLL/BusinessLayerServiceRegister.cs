using Medallion.Threading;
using Medallion.Threading.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using SubsTracker.BLL.Interfaces;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.BLL.Interfaces.Subscription;
using SubsTracker.BLL.Interfaces.User;
using SubsTracker.BLL.Mapper;
using SubsTracker.BLL.Services;
using SubsTracker.BLL.Services.Cache;
using SubsTracker.BLL.Services.Subscription;
using SubsTracker.BLL.Services.User;
using SubsTracker.DAL;
using SubsTracker.Messaging;

namespace SubsTracker.BLL;

public static class BusinessLayerServiceRegister
{
    public static IServiceCollection RegisterBusinessLayerDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterDataLayerDependencies(configuration)
            .RegisterMessagingLayerDependencies(configuration)
            .AddAutoMapper(_ => { }, typeof(DtoMappingProfile).Assembly)
            .AddScoped(typeof(IService<,,,,>), typeof(Service<,,,,>))
            .AddScoped<ISubscriptionService, SubscriptionService>()
            .AddScoped<IUserService, UserService>()
            .AddScoped<IUserGroupService, UserGroupService>()
            .AddScoped<IGroupMemberService, GroupMemberService>();

        services.AddStackExchangeRedisCache(option =>
        {
            option.Configuration = configuration["Redis"];
            option.InstanceName = "Redis_";
        });

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisConnectionString = configuration["Redis"] ?? "localhost:5140";
            return ConnectionMultiplexer.Connect(redisConnectionString);
        });

        services.AddSingleton<IDistributedLockProvider>(sp =>
        {
            var multiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
            return new RedisDistributedSynchronizationProvider(multiplexer.GetDatabase());
        });

        services.AddScoped<IDatabase>(sp =>
        {
            var multiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
            return multiplexer.GetDatabase();
        });

        services.AddScoped<ICacheService, CacheService>();

        return services;
    }

}
