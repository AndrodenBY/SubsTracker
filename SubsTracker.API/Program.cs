using Scalar.AspNetCore;
using SubsTracker.API.Middlewares.ExceptionHandling;
using SubsTracker.API.Middlewares.Session;

namespace SubsTracker.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
        });

        builder.Configuration
            .AddJsonFile("appsettings.json", false)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true)
            .AddEnvironmentVariables();

        if (!builder.Environment.IsEnvironment("IntegrationTest"))
        {
            builder.Configuration.AddUserSecrets<Program>();
        }

        builder.Services.AddOpenApi();
        builder.Services.RegisterApplicationLayerDependencies(builder.Configuration);

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options => 
            {
                options.WithTitle("SubsTracker API")
                    .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
            });
        }
        
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.UseHttpsRedirection();
        //app.UseStaticFiles();
        app.UseRouting();
        app.UseCors();

        app.UseAuthentication();
        app.UseMiddleware<SessionMiddleware>();
        app.UseAuthorization();

        app.MapControllers();

        await app.RunAsync();
    }
}
