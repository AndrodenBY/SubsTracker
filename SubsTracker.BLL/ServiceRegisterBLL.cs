using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SubsTracker.BLL.Interfaces;
using SubsTracker.BLL.Services;
using SubsTracker.BLL.Services.Subscription;
using SubsTracker.BLL.Services.User;
using SubsTracker.DAL;
using SubsTracker.DAL.Interfaces;
using SubsTracker.DAL.Repository;
using SubsTracker.Domain.Interfaces;

namespace SubsTracker.BLL;

public static class ServiceRegisterBLL
{
    public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterContext(configuration);
        services.AddScoped(typeof(IService<,,,,>), typeof(Service<,,,,>));
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IGroupMemberService, GroupMemberService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserGroupService, UserGroupService>();
        return services;
    }
}