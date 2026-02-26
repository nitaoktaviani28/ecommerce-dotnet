using Ecommerce.MonitoringApp.Repository;
using Ecommerce.MonitoringApp.Observability;
using Microsoft.AspNetCore.Http;
using OpenTelemetry.Trace;

namespace Ecommerce.MonitoringApp.Handlers;

public static class SuccessHandler
{
    public static async Task Handle(HttpContext context)
    {
        var start = DateTime.UtcNow;
        var tracer = TracerProvider.Default.GetTracer("handlers");
        using var span = tracer.StartActiveSpan("success_handler");

        int.TryParse(context.Request.Query["order_id"], out var orderId);

        var order = await PostgresRepository.GetOrderAsync(orderId, context.RequestAborted);
        var product = await PostgresRepository.GetProductAsync(order.ProductId, context.RequestAborted);

        AppMetrics.HttpRequestsTotal
            .WithLabels("GET", "/success", "200")
            .Inc();

        AppMetrics.HttpRequestDuration
            .WithLabels("GET", "/success")
            .Observe((DateTime.UtcNow - start).TotalSeconds);

        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync("<h1>Order Success</h1>");
    }
}
