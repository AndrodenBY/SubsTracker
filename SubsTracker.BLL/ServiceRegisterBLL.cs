using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SubsTracker.BLL.Services;
using SubsTracker.DAL;
using SubsTracker.Domain.Interfaces;

namespace SubsTracker.BLL;

public static class ServiceRegisterBLL
{
    public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterContext(configuration);
        services.AddScoped(typeof(IService<,,,>), typeof(ServiceBase<,,,>));
        return services;
    }
}