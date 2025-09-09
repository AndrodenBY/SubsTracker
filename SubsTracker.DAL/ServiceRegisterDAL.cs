using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SubsTracker.DAL.Interfaces;
using SubsTracker.DAL.Repository;
using SubsTracker.Domain.Interfaces;

namespace SubsTracker.DAL;

public static class ServiceRegisterDAL
{
    public static IServiceCollection RegisterContext(this IServiceCollection services, IConfiguration configuration)
    {
        var postgreConnectionString = configuration["PostgreConnectionString"];
        
        services.AddDbContext<SubsDbContext>(options =>
            options.UseNpgsql(postgreConnectionString));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<ISubscriptionHistoryRepository, SubscriptionHistoryRepository>();
        return services;
    }
}
