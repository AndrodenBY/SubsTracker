// using Microsoft.AspNetCore.Hosting;
// using Microsoft.AspNetCore.Mvc.Testing;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Infrastructure; // <-- Важно для IDbContextOptionsConfigurationSource
// using Microsoft.Extensions.DependencyInjection;
// using SubsTracker.DAL;
// using System.Linq;
//
// namespace SubsTracker.IntegrationTests;
//
// public class TestsWebApplicationFactory : WebApplicationFactory<Program>
// {
//     protected override void ConfigureWebHost(IWebHostBuilder builder)
//     {
//         builder.ConfigureServices(services =>
//         {
//             // 1. Агрессивно удаляем ВСЕ существующие регистрации EF Core.
//             var descriptorsToRemove = services.Where(d => 
//                 // Удаляем DbContextOptions<T>
//                 d.ServiceType == typeof(DbContextOptions<SubsDbContext>) || 
//                 // Удаляем сам DbContext
//                 d.ServiceType == typeof(SubsDbContext) ||
//                 // Удаляем все общие сервисы, связанные с конфигурацией провайдеров
//                 d.ServiceType.Name.Contains("DatabaseProvider") ||
//                 d.ServiceType.Name.Contains("DbContextOptions") || 
//                 d.ServiceType.Name.Contains("IDbContextOptionsConfiguration") ||
//                 // Удаляем Npgsql-специфичные реализации через namespace
//                 d.ImplementationType?.Namespace?.Contains("Npgsql") == true
//             ).ToList();
//
//             foreach (var descriptor in descriptorsToRemove)
//             {
//                 services.Remove(descriptor);
//             }
//             
//             // 2. Добавляем тестовый SubsDbContext, использующий In-Memory
//             services.AddDbContext<SubsDbContext>(options =>
//             {
//                 // Используем уникальное имя, чтобы гарантировать изоляцию
//                 options.UseInMemoryDatabase("TestDatabase_" + Guid.NewGuid().ToString());
//             });
//
//             // 3. Инициализация базы данных
//             var sp = services.BuildServiceProvider();
//
//             using (var scope = sp.CreateScope())
//             {
//                 var scopedServices = scope.ServiceProvider;
//                 var db = scopedServices.GetRequiredService<SubsDbContext>();
//                 
//                 db.Database.EnsureCreated();
//             }
//         });
//     }
// }
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using SubsTracker.DAL;

namespace SubsTracker.IntegrationTests;

public class TestsWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTest");
        
        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            var sources = configBuilder.Sources.ToList();
            configBuilder.Sources.Clear();

            foreach (var source in sources)
            {
                var typeName = source.GetType().FullName ?? string.Empty;
                if (!typeName.Contains("UserSecrets", StringComparison.OrdinalIgnoreCase))
                {
                    configBuilder.Sources.Add(source);
                }
            }
        });

        builder.ConfigureServices(services =>
        {
            
            var descriptorsToRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<SubsDbContext>) ||
                d.ServiceType == typeof(SubsDbContext) ||
                d.ServiceType.Name.Contains("DatabaseProvider") ||
                d.ServiceType.Name.Contains("DbContextOptions") ||
                d.ServiceType.Name.Contains("IDbContextOptionsConfiguration") ||
                d.ImplementationType?.Namespace?.Contains("Npgsql") == true
            ).ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // ✅ Добавляем InMemory реализацию с уникальным именем
            services.AddDbContext<SubsDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDatabase_" + Guid.NewGuid());
            });

        });
    }
}
