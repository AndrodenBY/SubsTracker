using Hangfire;
using Serilog;
using Serilog.Sinks.Network;
using SubsTracker.Hangfire.DI;
using SubsTracker.Hangfire.Helpers;
using SubsTracker.Hangfire.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Service", "SubsTracker.Hangfire")
        .WriteTo.Console();

    var logstashOptions = context.Configuration.GetSection(LogstashOptions.SectionName).Get<LogstashOptions>();
    if (!string.IsNullOrWhiteSpace(logstashOptions?.Host))
    {
        loggerConfiguration.WriteTo.TCPSink(logstashOptions.Host, logstashOptions.Port);
    }
});

builder.Services.AddHangfireServices(builder.Configuration);
var app = builder.Build();

app.UseRecurringJobs();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new HangfireAuthFilter()],
    IgnoreAntiforgeryToken = true
});
app.Run();
