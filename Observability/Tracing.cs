using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Ecommerce.MonitoringApp.Observability;

public static class Tracing
{
    public static void Setup(WebApplicationBuilder builder)
    {
        var serviceName = Env.Get("OTEL_SERVICE_NAME", "ecommerce-net");
        var endpoint = Env.Get(
            "OTEL_EXPORTER_OTLP_ENDPOINT",
            "http://alloy.monitoring.svc.cluster.local:4318"
        );

        builder.Services.AddOpenTelemetry()
            .WithTracing(t =>
            {
                t.SetResourceBuilder(
                        ResourceBuilder.CreateDefault()
                            .AddService(serviceName)
                    )
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(o =>
                    {
                        o.Endpoint = new Uri(endpoint);
                    })
                    .SetSampler(new AlwaysOnSampler());
            });
    }
}
