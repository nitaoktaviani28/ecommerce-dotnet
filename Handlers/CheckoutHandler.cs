using Ecommerce.MonitoringApp.Repository;
using Microsoft.AspNetCore.Http;
using OpenTelemetry.Trace;
using Prometheus;

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
        var productId = int.Parse(form["product_id"]);
        var quantity = int.Parse(form["quantity"]);

        try
        {
            var product = await PostgresRepository.GetProductAsync(
                productId,
                context.RequestAborted
            );

            var total = product.Price * quantity;

            var orderId = await PostgresRepository.CreateOrderAsync(
                productId,
                quantity,
                total,
                context.RequestAborted
            );

            // metrics (padanan ordersCreatedTotal.Inc())
            Metrics.OrdersCreatedTotal.Inc();

            context.Response.Redirect($"/success?order_id={orderId}", false);
        }
        catch
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Checkout failed");
        }
        finally
        {
            RecordHttpMetrics(context, start);
        }
    }

    private static void RecordHttpMetrics(HttpContext ctx, DateTime start)
    {
        var duration = (DateTime.UtcNow - start).TotalSeconds;

        Metrics.HttpRequestsTotal
            .WithLabels(ctx.Request.Method, ctx.Request.Path, ctx.Response.StatusCode.ToString())
            .Inc();

        Metrics.HttpRequestDuration
            .WithLabels(ctx.Request.Method, ctx.Request.Path)
            .Observe(duration);
    }
}
