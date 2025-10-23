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
        
        var userName = configuration["RabbitMQ:UserName"];
        var password = configuration["RabbitMQ:Password"];
        var hostName = configuration["RabbitMQ:HostName"];
        var virtualHostName = configuration["RabbitMQ:VirtualHostName"];
        
        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.SetKebabCaseEndpointNameFormatter();
            busConfigurator.UsingRabbitMq((context, rabbitMqConfigurator) =>
            {
                rabbitMqConfigurator.Host(hostName, virtualHostName, configure =>
                {
                    configure.Username(userName);
                    configure.Password(password);
                });
                
                rabbitMqConfigurator.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
