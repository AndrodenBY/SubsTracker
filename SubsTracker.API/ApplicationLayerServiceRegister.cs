using System.Security.Claims;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using SubsTracker.API.Mapper;
using SubsTracker.API.Validators.User;
using SubsTracker.BLL;
using SubsTracker.Domain.Options;

namespace SubsTracker.API;

public static class ApplicationLayerServiceRegister
{
    public static void RegisterApplicationLayerDependencies(this IServiceCollection services, IConfiguration configuration)
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
            ));
        
        var auth0Section = configuration.GetSection(Auth0Options.SectionName);
        var auth0Options = auth0Section.Get<Auth0Options>();

        services.Configure<Auth0Options>(auth0Section);
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = auth0Options!.Authority;
                options.Audience = auth0Options.Audience;
                
                options.TokenValidationParameters = new()
                {
                    NameClaimType = ClaimTypes.NameIdentifier,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                };
            });
    }
}
