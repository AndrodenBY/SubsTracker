using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SubsTracker.BLL.Interfaces;
using SubsTracker.BLL.Interfaces.Subscription;
using SubsTracker.BLL.Interfaces.user;
using SubsTracker.BLL.Interfaces.User;
using SubsTracker.BLL.Mapper;
using SubsTracker.BLL.Services;
using SubsTracker.BLL.Services.Subscription;
using SubsTracker.BLL.Services.User;
using SubsTracker.DAL;

namespace SubsTracker.BLL;

public static class ServiceRegisterBLL
{
    public static IServiceCollection RegisterServicesBll(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterServicesDal(configuration);
        services.AddAutoMapper(cfg => { }, typeof(DtoMappingProfile).Assembly);
        services.AddScoped(typeof(IService<,,,,>), typeof(Service<,,,,>));
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserGroupService, UserGroupService>();
        return services;
    }
}
