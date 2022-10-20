using Microsoft.EntityFrameworkCore;
using NotificationService.Infrastructure;
using NotificationService.Services;
using MassTransit;
using Core.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCustomDistributedTracing("NotificationService", builder.Configuration);

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddDbContext<NotificationDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("NotificationDatabase")))
    .AddScoped<IDatabaseInitializer, DatabaseInitializer>()
    .AddScoped<IHealthService, HealthService>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<NotificationService.Consumers.UserCreatedEmailConsumer>();
    x.AddConsumer<NotificationService.Consumers.PaymentReceiptConsumer>();
    x.AddConsumer<NotificationService.Consumers.SecurityAlertConsumer>();
    x.AddConsumer<NotificationService.Consumers.PaymentFailedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        
        cfg.ReceiveEndpoint("notification-user-created", e =>
        {
            e.ConfigureConsumer<NotificationService.Consumers.UserCreatedEmailConsumer>(context);
        });
    });
});

var app = builder.Build();

await InitializeDatabaseAsync(app);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/info", () => Results.Ok(new { Service = "Notification", MessageBroker = "RabbitMQ" }));

app.MapGet("/stats", () => Results.Ok(new { EmailsSent = 154 }));

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
