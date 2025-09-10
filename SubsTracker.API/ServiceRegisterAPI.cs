using FluentValidation;
using FluentValidation.AspNetCore;
using SubsTracker.API.Mapper;
using SubsTracker.API.Validators.User;
using SubsTracker.BLL;

namespace SubsTracker.API;

public static class ServiceRegisterAPI
{
    public static IServiceCollection RegisterServicesApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterServicesBll(configuration);
        services.AddAutoMapper(cfg => { }, typeof(ViewModelMappingProfile).Assembly);
        services.AddFluentValidationAutoValidation()
            .AddValidatorsFromAssemblyContaining<CreateUserDtoValidator>();
        return services;
    }
}
