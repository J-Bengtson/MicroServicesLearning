using Microsoft.EntityFrameworkCore;
using UserService.Infrastructure;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserService.Consumers.PaymentProcessedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("UserDatabase")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/user", () => Results.Ok(new
{
    message = "UserService is up",
    version = "1.0"
}));

app.MapGet("/info", () => Results.Ok(new { Service = "User", Version = "1.0", Database = "Connected" }));

app.MapPost("/users", async (UserService.Domain.User user, UserDbContext db, MassTransit.IPublishEndpoint publishEndpoint) =>
{
    // Mock save user to DB
    db.Users.Add(user);
    await db.SaveChangesAsync();

    // Publish Integration Event to RabbitMQ
    await publishEndpoint.Publish(new Core.Events.UserCreatedIntegrationEvent
    {
        UserId = user.Id,
        Username = user.Username,
        Email = user.Email,
        PasswordHash = user.PasswordHash
    });

    return Results.Created($"/user/{user.Id}", user);
});

app.Run();
