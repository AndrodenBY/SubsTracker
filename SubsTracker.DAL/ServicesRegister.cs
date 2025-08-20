using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SubsTracker.DAL.Models;
using SubsTracker.DAL.Repository;
using SubsTracker.Domain;

namespace SubsTracker.DAL;

public static class ServicesRegister
{
    public static IServiceCollection RegisterContext(this IServiceCollection services, IConfiguration configuration)
    {
        var postgreConnectionString = configuration["PostgreConnectionString"];
        if (string.IsNullOrEmpty(postgreConnectionString))
        {
            throw new InvalidOperationException("Connection string 'PostgreConnectionString' not found.");
        }
        
        services.AddDbContext<SubsDbContext>(options =>
            options.UseNpgsql(postgreConnectionString));

        services.AddScoped<IRepository<BaseModel>, Repository<BaseModel>>();
        
        return services;
    }
}