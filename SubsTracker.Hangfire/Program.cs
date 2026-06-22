using Hangfire;
using SubsTracker.Hangfire.DI;
using SubsTracker.Hangfire.Helpers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHangfireServices(builder.Configuration);
var app = builder.Build();

app.UseRecurringJobs();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new HangfireAuthFilter()],
    IgnoreAntiforgeryToken = true
});
app.Run();
