using Scalar.AspNetCore;
using Serilog;
using Serilog.Sinks.Network;
using SubsTracker.API.Middlewares.ExceptionHandling;
using SubsTracker.API.Options;

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

        builder.Host.UseSerilog((context, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Service", "SubsTracker.API")
                .WriteTo.Console();

            var logstashOptions = context.Configuration.GetSection(LogstashOptions.SectionName).Get<LogstashOptions>();
            if (!string.IsNullOrWhiteSpace(logstashOptions?.Host))
            {
                loggerConfiguration.WriteTo.TCPSink(logstashOptions.Host, logstashOptions.Port);
            }
        });

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
        app.UseRouting();
        app.UseCors();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        await app.RunAsync();
    }
}
