using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using SubsTracker.Messaging.Interfaces;
using SubsTracker.Messaging.Services;

namespace SubsTracker.IntegrationTests.Configuration.ServiceConfigs;

public static class MassTransitTestConfig
{
    public static IServiceCollection ReplaceMassTransit(this IServiceCollection services)
    {
        services.AddMassTransitTestHarness();

        services.RemoveAll<IMessageService>();
        services.AddScoped<IMessageService>(sp => 
        {
            var publishEndpoint = sp.GetRequiredService<IPublishEndpoint>();
            var logger = sp.GetRequiredService<ILogger<MessageService>>();
            return new MessageService(publishEndpoint, logger);
        });

        return services;
    }
}
