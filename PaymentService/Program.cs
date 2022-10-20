using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using PaymentService.Infrastructure;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PaymentService.Domain;
using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Core.Events;
using Core.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCustomDistributedTracing("PaymentService", builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PaymentDatabase")));

builder.Services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true
    };
});
builder.Services.AddAuthorization();

builder.Services.AddMassTransit(x =>
{
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

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
    await initializer.InitializeAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/info", () => Results.Ok(new { Service = "Payment", Secure = true }));

app.MapGet("/stats", async (PaymentDbContext db) => 
{
    var transactionCount = await db.PaymentTransactions.CountAsync();
    var totalVolume = await db.PaymentTransactions.SumAsync(p => p.Amount);
    return Results.Ok(new { TransactionCount = transactionCount, TotalVolume = totalVolume });
});

app.MapPost("/payments/process", async (PaymentRequest request, PaymentDbContext db, IPublishEndpoint publishEndpoint, HttpContext httpContext) =>
{
    var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
    {
        return Results.Unauthorized();
    }

    var transaction = PaymentTransaction.Create(userId, request.Amount);

    db.PaymentTransactions.Add(transaction);
    await db.SaveChangesAsync();

    if (transaction.IsSuccessful)
    {
        await publishEndpoint.Publish(new PaymentProcessedIntegrationEvent
        {
            PaymentId = transaction.Id,
            UserId = transaction.UserId,
            Amount = transaction.Amount,
            IsSuccessful = true
        });
        return Results.Ok(Core.Domain.ApiResponse<PaymentTransaction>.CreateSuccess(transaction));
    }
    else
    {
        await publishEndpoint.Publish(new PaymentFailedIntegrationEvent
        {
            PaymentId = transaction.Id,
            UserId = transaction.UserId,
            Amount = transaction.Amount,
            Reason = transaction.FailureReason ?? "Unknown Error",
            FailedAt = DateTime.UtcNow
        });
        return Results.BadRequest(Core.Domain.ApiResponse<object>.CreateFail(transaction.FailureReason ?? "Unknown Error"));
    }
}).RequireAuthorization();

app.Run();

public class PaymentRequest
{
    public decimal Amount { get; set; }
}
