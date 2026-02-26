using Microsoft.Extensions.Logging;

namespace Ecommerce.MonitoringApp.Observability;

public static class ObservabilityInit
{
    public static void Init(WebApplicationBuilder builder)
    {
        var logger = LoggerFactory
            .Create(b => b.AddConsole())
            .CreateLogger("observability");

        logger.LogInformation("Initializing observability...");

        Tracing.Setup(builder);
        Metrics.Setup(builder);
        Profiling.Setup();

        logger.LogInformation("Observability initialized");
    }
}
