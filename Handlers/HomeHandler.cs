using Ecommerce.MonitoringApp.Repository.Models;
using Ecommerce.MonitoringApp.Repository;
using Microsoft.AspNetCore.Http;
using OpenTelemetry.Trace;

namespace Ecommerce.MonitoringApp.Handlers;

public static class HomeHandler
{
    public static async Task Handle(HttpContext context)
    {
        var tracer = TracerProvider.Default.GetTracer("handlers");
        using var span = tracer.StartActiveSpan("home_handler");

        try
        {
            var products = await PostgresRepository.GetProductsAsync(context.RequestAborted);

            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.WriteAsync(RenderIndexHtml(products));
        }
        catch
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Failed to get products");
        }
    }

    // sementara: render manual (nanti bisa diganti Razor)
    private static string RenderIndexHtml(IEnumerable<Product> products)
    {
        var html = """
        <html>
        <head><title>E-Commerce Store</title></head>
        <body>
        <h1>🛒 E-Commerce Store</h1>
        """;

        foreach (var p in products)
        {
            html += $"""
            <div>
              <h3>{p.Name}</h3>
              <p>Rp {p.Price}</p>
              <form action="/checkout" method="POST">
                <input type="hidden" name="product_id" value="{p.Id}" />
                <input type="number" name="quantity" value="1" min="1" />
                <button type="submit">Buy</button>
              </form>
            </div>
            <hr/>
            """;
        }

        html += "</body></html>";
        return html;
    }
}
