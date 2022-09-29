using Microsoft.EntityFrameworkCore;
using AuthService.Infrastructure;
using AuthService.Services;

using MassTransit;
using AuthService.Consumers;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddDbContext<AuthDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("AuthDatabase")))
    .AddScoped<IDatabaseInitializer, DatabaseInitializer>()
    .AddScoped<IHealthService, HealthService>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserCreatedConsumer>();

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

app.MapGet("/auth", () => Results.Ok(new
{
    message = "AuthService is initialized",
    version = "1.0"
}));

app.Run();

static async Task InitializeDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
    await initializer.InitializeAsync();
}
