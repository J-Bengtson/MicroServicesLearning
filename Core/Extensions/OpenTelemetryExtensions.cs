using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;

namespace Core.Extensions
{
    public static class OpenTelemetryExtensions
    {
        public static IServiceCollection AddCustomDistributedTracing(this IServiceCollection services, string serviceName, IConfiguration configuration)
        {
            var zipkinEndpoint = configuration["Zipkin__Endpoint"] ?? "http://localhost:9411/api/v2/spans";

            services.AddOpenTelemetry()
                .WithTracing(tracerProviderBuilder =>
                {
                    tracerProviderBuilder
                        .AddSource(serviceName)
                        .AddSource("MassTransit") // Rastrea mensagens na fila automaticamente!
                        .SetResourceBuilder(
                            ResourceBuilder.CreateDefault()
                                .AddService(serviceName: serviceName, serviceVersion: "1.0.0"))
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddZipkinExporter(options =>
                        {
                            options.Endpoint = new Uri(zipkinEndpoint);
                        });
                });

            return services;
        }
    }
}
