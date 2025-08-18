using Microsoft.EntityFrameworkCore;

namespace SubsTracker.DAL;

public static class ServicesRegister
{
    public static IServiceCollection RegisterContext(this IServiceCollection services, IConfiguration configuration)
    {
        var postgreConnectionString = configuration.GetConnectionString("PostgreConnectionString");
        services.AddDbContext<SubsDbContext>(options =>
            options.UseNpgsql(postgreConnectionString));
        
        return services;
    }
    
}