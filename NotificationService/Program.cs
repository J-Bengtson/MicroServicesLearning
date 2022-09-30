using Microsoft.EntityFrameworkCore;
using NotificationService.Infrastructure;
using NotificationService.Services;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddDbContext<NotificationDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("NotificationDatabase")))
    .AddScoped<IDatabaseInitializer, DatabaseInitializer>()
    .AddScoped<IHealthService, HealthService>();

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

await InitializeDatabaseAsync(app);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", async (IHealthService healthService) =>
{
    var health = await healthService.CheckHealthAsync();
    return Results.Ok(health);
});

app.MapGet("/notifications", () => Results.Ok(new
{
    message = "NotificationService is initialized",
    version = "1.0"
}));

app.Run();

static async Task InitializeDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
    await initializer.InitializeAsync();
}
