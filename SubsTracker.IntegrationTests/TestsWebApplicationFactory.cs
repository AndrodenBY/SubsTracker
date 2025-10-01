using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SubsTracker.DAL;

namespace SubsTracker.IntegrationTests;

public class TestsWebApplicationFactory : WebApplicationFactory<Program>
{
    internal readonly WebApplicationFactory<Program> WebHost;
    
    public TestsWebApplicationFactory()
    {
        WebHost = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
            {
                var descriptorsToRemove = services.Where(descriptor =>
                    descriptor.ServiceType == typeof(DbContextOptions<SubsDbContext>) ||
                    descriptor.ImplementationType?.Namespace?.Contains("Npgsql") == true
                ).ToList();
    
                foreach (var descriptor in descriptorsToRemove)
                {
                    services.Remove(descriptor);
                }
    
                services.AddDbContext<SubsDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbContextTest");
                });
            }));
    }
    
    
    // protected override void ConfigureWebHost(IWebHostBuilder builder)
    // {
    //     builder.ConfigureServices(services =>
    //     {
    //         var descriptorsToRemove = services.Where(descriptor =>
    //             descriptor.ServiceType == typeof(DbContextOptions<SubsDbContext>) ||
    //             descriptor.ImplementationType?.Namespace?.Contains("Npgsql") == true
    //             ).ToList();
    //
    //         foreach (var descriptor in descriptorsToRemove)
    //         {
    //             services.Remove(descriptor);
    //         }
    //         
    //
    //         services.AddDbContext<SubsDbContext>(options =>
    //         {
    //             options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid());
    //         });
    //     });
    // }
}





// protected override void ConfigureWebHost(IWebHostBuilder builder)
// {   
//     builder.ConfigureServices(services =>
//     {
//         var descriptorsToRemove = services
//             .Where(d =>
//                 d.ServiceType == typeof(DbContextOptions<SubsDbContext>) ||
//                 d.ServiceType == typeof(DbContextOptions) ||
//                 d.ServiceType == typeof(DbContext) ||
//                 d.ServiceType.Name.Contains("DatabaseProvider") ||
//                 d.ImplementationType?.Namespace?.Contains("Npgsql") == true
//             ).ToList();
//
//         foreach (var descriptor in descriptorsToRemove)
//         {
//             services.Remove(descriptor);
//         }
//         
//         var efServices = new ServiceCollection();
//         efServices.AddEntityFrameworkInMemoryDatabase();
//         var efProvider = efServices.BuildServiceProvider();
//         
//         services.AddDbContext<SubsDbContext>(options =>
//         {
//             options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid());
//             options.UseInternalServiceProvider(efProvider);
//         });
//         
//         var sp = services.BuildServiceProvider();
//
//         using var scope = sp.CreateScope();
//         var scopedServices = scope.ServiceProvider;
//         var db = scopedServices.GetRequiredService<SubsDbContext>();
//         db.Database.EnsureCreated();
//     });
// }
