using Ecommerce.MonitoringApp.Repository;
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

        if (!int.TryParse(context.Request.Query["order_id"], out var orderId))
        {
            context.Response.StatusCode = 400;
            return;
        }

        try
        {
            var order = await PostgresRepository.GetOrderAsync(orderId, context.RequestAborted);
            var product = await PostgresRepository.GetProductAsync(order.ProductId, context.RequestAborted);

            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.WriteAsync($"""
                <html>
                <body>
                  <h1>✅ Order Success</h1>
                  <p>Order ID: {order.Id}</p>
                  <p>Product: {product.Name}</p>
                  <p>Quantity: {order.Quantity}</p>
                  <p>Total: Rp {order.Total}</p>
                  <a href="/">Back</a>
                </body>
                </html>
            """);
        }
        catch
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync("Order not found");
        }
        finally
        {
            var duration = (DateTime.UtcNow - start).TotalSeconds;
            Metrics.HttpRequestsTotal
                .WithLabels(context.Request.Method, context.Request.Path, context.Response.StatusCode.ToString())
                .Inc();

            Metrics.HttpRequestDuration
                .WithLabels(context.Request.Method, context.Request.Path)
                .Observe(duration);
        }
    }
}
