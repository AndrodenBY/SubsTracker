using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SubsTracker.Messaging.Interfaces;
using SubsTracker.Messaging.Services;

namespace SubsTracker.Messaging;

public static class MessagingLayerRegister
{
    public static IServiceCollection RegisterMessagingLayerDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IMessageService, MessageService>();
        
        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.SetKebabCaseEndpointNameFormatter();
            busConfigurator.UsingRabbitMq((context, rabbitMqConfigurator) =>
            {
                rabbitMqConfigurator.Host(configuration["RabbitMQ:HostName"], configuration["RabbitMQ:VirtualHostName"], configure =>
                {
                    configure.Username(configuration["RabbitMQ:UserName"]);
                    configure.Password(configuration["RabbitMQ:Password"]);
                });
            });
        });

        return services;
    }
}
