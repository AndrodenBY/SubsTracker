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

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = Environment.GetEnvironmentVariable("Auth0__Domain");
                options.Audience = Environment.GetEnvironmentVariable("Auth0__Audience");
            });

        return services;
    }
}
