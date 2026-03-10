using SubsTracker.API.DI;
using SubsTracker.BLL;

namespace SubsTracker.API;

public static class ApplicationLayerServiceRegister
{
    public static void RegisterApplicationLayerDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterBusinessLayerDependencies(configuration)
            .AddInfrastructureDependencies(configuration)
            .AddAuthorizationDependencies()
            .AddResilienceDependencies();
    }
}
