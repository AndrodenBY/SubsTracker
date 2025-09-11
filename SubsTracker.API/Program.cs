using FluentValidation.AspNetCore;
using SubsTracker.API.Middlewares.ExceptionHandling;
using SubsTracker.BLL;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>()
    .AddEnvironmentVariables();

builder.Services.AddAutoMapper(cfg => { }, typeof(Program).Assembly);
builder.Services.RegisterServices(builder.Configuration);

builder.Services.AddControllers()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining(typeof(Program)));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.Run();
