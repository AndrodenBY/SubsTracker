using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Options;
using SubsTracker.API.Extension;
using SubsTracker.API.Mapper;
using SubsTracker.API.Options;
using SubsTracker.API.Validators.User;

namespace SubsTracker.API.DI;

public static class InfrastructureDependencies
{
    public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services)
    {
        services.AddAutoMapper(_ => { }, typeof(ViewModelMappingProfile).Assembly)
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });
        
        services.AddFluentValidationAutoValidation()
            .AddValidatorsFromAssemblyContaining<CreateUserDtoValidator>();

        services.RegisterOptions<CorsOptions>(CorsOptions.SectionName);
        
        services.AddCors(options =>
            options.AddDefaultPolicy(policy =>
            {
                var serviceProvider = services.BuildServiceProvider();
                var corsOptions = serviceProvider.GetRequiredService<IOptions<CorsOptions>>().Value;
                
                policy.WithOrigins(corsOptions.AllowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .WithExposedHeaders("Content-Disposition");
            }));

        return services;
    }
}
