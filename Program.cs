using Microsoft.EntityFrameworkCore;
using SubsTracker;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

string? postgreConnectionString = builder.Configuration.GetConnectionString("PostgreConnectionString");
builder.Services.AddDbContext<SubsDbContext>(options => options.UseNpgsql(postgreConnectionString));

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.Run();