using Medallion.Threading;
using Medallion.Threading.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
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

        services
            .AddStackExchangeRedisCache(redisOptions =>
            {
                redisOptions.Configuration = configuration["Redis"];
                redisOptions.InstanceName = "Redis_";
            })
            .AddSingleton<IConnectionMultiplexer>(serviceProvider =>
                ConnectionMultiplexer.Connect(configuration["Redis"] ?? "localhost:5140"))
            .AddSingleton<IDistributedLockFactory>(serviceProvider =>
                RedLockFactory.Create(new List<RedLockMultiplexer>
                {
                    new(serviceProvider.GetRequiredService<IConnectionMultiplexer>())
                }));

        services.AddScoped<ICacheService, CacheService>();

        return services;
    }

}
