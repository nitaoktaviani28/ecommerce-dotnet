namespace Ecommerce.MonitoringApp.Observability;

public static class ObservabilityInit
{
    public static void Init(WebApplicationBuilder builder)
    {
        Tracing.Setup(builder);
        AppMetrics.Setup(builder);
        Profiling.Setup();
    }
}
