using System.Text.Json.Serialization;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using SubsTracker.API.Mapper;
using SubsTracker.API.Validators.User;
using SubsTracker.BLL;

namespace SubsTracker.API;

public static class ApplicationLayerServiceRegister
{
    public static IServiceCollection RegisterApplicationLayerDependencies(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.RegisterBusinessLayerDependencies(configuration)
            .AddAutoMapper(cfg => { }, typeof(ViewModelMappingProfile).Assembly)
            .AddControllers()
            .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<CreateUserDtoValidator>());

        services.AddCors(options =>
            options.AddDefaultPolicy(policy =>
                policy.WithOrigins("http://localhost:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
            ));
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = Environment.GetEnvironmentVariable("Auth0__Domain");
                options.Audience = Environment.GetEnvironmentVariable("Auth0__Audience");
            });
        
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        return services;
    }
}
