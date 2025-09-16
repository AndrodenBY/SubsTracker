using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SubsTracker.BLL.Interfaces;
using SubsTracker.BLL.Interfaces.Subscription;
using SubsTracker.BLL.Interfaces.User;
using SubsTracker.BLL.Mapper;
using SubsTracker.BLL.Services;
using SubsTracker.BLL.Services.Subscription;
using SubsTracker.BLL.Services.User;
using SubsTracker.DAL;

namespace SubsTracker.BLL;

public static class BusinessLayerServiceRegister
{
    public static IServiceCollection RegisterBusinessLayerDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterDataLayerDependencies(configuration)
        .AddAutoMapper(cfg => { }, typeof(DtoMappingProfile).Assembly)
        .AddScoped(typeof(IService<,,,,>), typeof(Service<,,,,>))
        .AddScoped<ISubscriptionService, SubscriptionService>()
        .AddScoped<IUserService, UserService>()
        .AddScoped<IUserGroupService, UserGroupService>();
        return services;
    }
}
