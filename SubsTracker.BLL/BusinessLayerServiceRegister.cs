using System.Reflection;
using DispatchR.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SubsTracker.BLL.DI;
using SubsTracker.BLL.Helpers.Policy;
using SubsTracker.BLL.Interfaces;
using SubsTracker.BLL.Mapper;
using SubsTracker.BLL.Mediator.Handlers.UpcomingBills;
using SubsTracker.BLL.Services;
using SubsTracker.DAL;
using SubsTracker.Messaging;

namespace SubsTracker.BLL;

public static class BusinessLayerServiceRegister
{
    public static IServiceCollection RegisterBusinessLayerDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterDataLayerDependencies(configuration)
            .RegisterMessagingLayerDependencies(configuration)
            .AddCacheDependencies(configuration)
            .AddAutoMapper(_ => { }, Assembly.GetExecutingAssembly())
            .AddAutoMapper(_ => { }, typeof(DtoMappingProfile).Assembly)
            .AddScoped(typeof(IService<,,,,>), typeof(Service<,,,,>))
            .AddScoped<ISubscriptionService, SubscriptionService>()
            .AddScoped<IUserService, UserService>()
            .AddScoped<IGroupService, GroupService>()
            .AddScoped<IMemberService, MemberService>()
            .AddScoped<IMemberPolicyChecker, MemberPolicyChecker>();

        services.AddDispatchR(options =>
        {
            options.Assemblies.Add(typeof(GetUpcomingBillsHandler).Assembly);
            options.RegisterNotifications = true;
            options.RegisterPipelines = true;
        });
        
        return services;
    }
}
