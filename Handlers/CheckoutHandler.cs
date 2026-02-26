using Ecommerce.MonitoringApp.Repository;
using Ecommerce.MonitoringApp.Observability;
using Microsoft.AspNetCore.Http;
using OpenTelemetry.Trace;

namespace Ecommerce.MonitoringApp.Handlers;

public static class CheckoutHandler
{
    public static async Task Handle(HttpContext context)
    {
        var start = DateTime.UtcNow;
        var tracer = TracerProvider.Default.GetTracer("handlers");
        using var span = tracer.StartActiveSpan("checkout_handler");

        if (!HttpMethods.IsPost(context.Request.Method))
        {
            context.Response.StatusCode = 405;
            return;
        }

        var form = await context.Request.ReadFormAsync();
        int.TryParse(form["product_id"], out var productId);
        int.TryParse(form["quantity"], out var quantity);

        var product = await PostgresRepository.GetProductAsync(
            productId, context.RequestAborted);

        var total = product.Price * quantity;

        var orderId = await PostgresRepository.CreateOrderAsync(
            productId, quantity, total, context.RequestAborted);

        AppMetrics.OrdersCreatedTotal.Inc();

        AppMetrics.HttpRequestsTotal
            .WithLabels("POST", "/checkout", "303")
            .Inc();

        AppMetrics.HttpRequestDuration
            .WithLabels("POST", "/checkout")
            .Observe((DateTime.UtcNow - start).TotalSeconds);

        context.Response.Redirect($"/success?order_id={orderId}", false);
    }
}
