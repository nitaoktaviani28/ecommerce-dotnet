using Prometheus;

namespace Ecommerce.MonitoringApp.Observability;

public static class Metrics
{
    public static readonly Counter HttpRequestsTotal =
        Prometheus.Metrics.CreateCounter(
            "http_requests_total",
            "Total HTTP requests",
            new CounterConfiguration
            {
                LabelNames = new[] { "method", "endpoint", "status" }
            });

    public static readonly Histogram HttpRequestDuration =
        Prometheus.Metrics.CreateHistogram(
            "http_request_duration_seconds",
            "HTTP request duration",
            new HistogramConfiguration
            {
                LabelNames = new[] { "method", "endpoint" }
            });

    public static readonly Counter OrdersCreatedTotal =
        Prometheus.Metrics.CreateCounter(
            "orders_created_total",
            "Total orders created"
        );

    public static void Setup(WebApplicationBuilder builder)
    {
        // expose /metrics
        builder.Services.AddSingleton<ICollectorRegistry>(Metrics.DefaultRegistry);
    }
}
