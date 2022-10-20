using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;

using Polly;
using Polly.Extensions.Http;
using Core.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCustomDistributedTracing("GatewayService", builder.Configuration);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

builder.Services.AddHttpClient(Microsoft.Extensions.Options.Options.DefaultName)
    .AddPolicyHandler(retryPolicy)
    .AddPolicyHandler(circuitBreakerPolicy);

var app = builder.Build();

app.MapGet("/api/system/overview", async (IHttpClientFactory clientFactory, IConfiguration config) =>
{
    var client = clientFactory.CreateClient();
    
    // We will extract destinations from configuration to make the calls
    var userUrl = config["ReverseProxy:Clusters:userCluster:Destinations:destination1:Address"] + "info";
    var authUrl = config["ReverseProxy:Clusters:authCluster:Destinations:destination1:Address"] + "info";
    var paymentUrl = config["ReverseProxy:Clusters:paymentCluster:Destinations:destination1:Address"] + "info";
    var notificationUrl = config["ReverseProxy:Clusters:notificationCluster:Destinations:destination1:Address"] + "info";

    var safeGet = async (string url) =>
    {
        try
        {
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<object>(content);
            }
            return new { Status = "Error", StatusCode = response.StatusCode };
        }
        catch (Exception ex)
        {
            return new { Status = "Unreachable", Error = ex.Message };
        }
    };

    var userTask = safeGet(userUrl);
    var authTask = safeGet(authUrl);
    var paymentTask = safeGet(paymentUrl);
    var notificationTask = safeGet(notificationUrl);

    await Task.WhenAll(userTask, authTask, paymentTask, notificationTask);

    return Results.Ok(new
    {
        AggregatedStatus = "Success",
        Services = new
        {
            UserService = userTask.Result,
            AuthService = authTask.Result,
            PaymentService = paymentTask.Result,
            NotificationService = notificationTask.Result
        }
    });
});

app.MapGet("/api/reports/system-stats", async (IHttpClientFactory clientFactory, IConfiguration config) =>
{
    var client = clientFactory.CreateClient();
    
    var userUrl = config["ReverseProxy:Clusters:userCluster:Destinations:destination1:Address"] + "stats";
    var authUrl = config["ReverseProxy:Clusters:authCluster:Destinations:destination1:Address"] + "stats";
    var paymentUrl = config["ReverseProxy:Clusters:paymentCluster:Destinations:destination1:Address"] + "stats";
    var notificationUrl = config["ReverseProxy:Clusters:notificationCluster:Destinations:destination1:Address"] + "stats";
    var invoicesUrl = config["ReverseProxy:Clusters:paymentCluster:Destinations:destination1:Address"] + "payments/invoices";

    var safeGet = async (string url) =>
    {
        try
        {
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<object>(content);
            }
            return new { Status = "Error", StatusCode = response.StatusCode };
        }
        catch (Exception ex)
        {
            return new { Status = "Unreachable", Error = ex.Message };
        }
    };

    var userTask = safeGet(userUrl);
    var authTask = safeGet(authUrl);
    var paymentTask = safeGet(paymentUrl);
    var notificationTask = safeGet(notificationUrl);
    var invoicesTask = safeGet(invoicesUrl);

    await Task.WhenAll(userTask, authTask, paymentTask, notificationTask, invoicesTask);

    return Results.Ok(new
    {
        ReportType = "Global System Stats",
        Timestamp = DateTime.UtcNow,
        Data = new
        {
            Users = userTask.Result,
            Security = authTask.Result,
            Finance = paymentTask.Result,
            Invoices = invoicesTask.Result,
            Notifications = notificationTask.Result
        }
    });
});

app.MapReverseProxy();

app.Run();
