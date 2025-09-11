using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.DAL.Repository;

namespace SubsTracker.DAL;

public static class ServiceRegisterDAL
{
    public static IServiceCollection RegisterServicesDal(this IServiceCollection services, IConfiguration configuration)
    {
        var postgreConnectionString = configuration["PostgreConnectionString"];
        
        services.AddDbContext<SubsDbContext>(options =>
            options.UseNpgsql(postgreConnectionString))
        .AddScoped(typeof(IRepository<>), typeof(Repository<>))
        .AddScoped<ISubscriptionRepository, SubscriptionRepository>()
        .AddScoped<ISubscriptionHistoryRepository, SubscriptionHistoryRepository>();
        return services;
    }
}
