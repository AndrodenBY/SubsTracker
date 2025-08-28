using SubsTracker.API.ExceptionHandling;
using SubsTracker.BLL;
using SubsTracker.BLL.Mapper;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>()
    .AddEnvironmentVariables();

builder.Services.AddAutoMapper(cfg => { }, typeof(DtoMappingProfile).Assembly);
builder.Services.RegisterServices(builder.Configuration);

builder.Services.AddControllersWithViews();

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