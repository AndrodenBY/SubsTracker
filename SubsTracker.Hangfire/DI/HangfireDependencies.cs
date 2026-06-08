using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using SubsTracker.BLL;
using SubsTracker.Hangfire.Jobs;
using SubsTracker.Hangfire.Options;

namespace SubsTracker.Hangfire.DI;

public static class HangfireDependencies
{
    public static void AddHangfireServices(this IServiceCollection services, IConfiguration configuration)
    {
        var hangfireOptions = configuration
            .GetSection(HangfireOptions.SectionName)
            .Get<HangfireOptions>();
        
        var connectionString = configuration["MongoDbConnectionString"];
        
        services.AddHangfire(hangfireConfiguration => hangfireConfiguration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseMongoStorage(
                connectionString,
                hangfireOptions!.DatabaseName,
                new MongoStorageOptions
                {
                    MigrationOptions = new MongoMigrationOptions
                    {
                        MigrationStrategy = new MigrateMongoMigrationStrategy(),
                        BackupStrategy = new CollectionMongoBackupStrategy()
                    }
                }
            )
        ).AddHangfireServer();
        
        services.RegisterBusinessLayerDependencies(configuration);
    }
    
    public static WebApplication UseRecurringJobs(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var recurringJobManager =
            scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

        recurringJobManager.AddOrUpdate<SubscriptionExpirationJob>(
            "subscription-expiration",
            job => job.ProcessExpiredSubscription(CancellationToken.None),
            Cron.Daily);

        return app;
    }
}


