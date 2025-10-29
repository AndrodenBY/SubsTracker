using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SubsTracker.Messaging.Interfaces;
using SubsTracker.Messaging.Options;
using SubsTracker.Messaging.Services;

namespace SubsTracker.Messaging;

public static class MessagingLayerRegister
{
    public static IServiceCollection RegisterMessagingLayerDependencies(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));

        services.AddScoped<IMessageService, MessageService>();

        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.SetKebabCaseEndpointNameFormatter();
            busConfigurator.UsingRabbitMq((context, rabbitMqConfigurator) =>
            {
                var rabbitMqOptions = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
                rabbitMqConfigurator.Host(rabbitMqOptions.HostName, rabbitMqOptions.VirtualHostName, configure =>
                {
                    configure.Username(rabbitMqOptions.UserName);
                    configure.Password(rabbitMqOptions.Password);
                });

                rabbitMqConfigurator.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
