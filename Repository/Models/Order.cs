namespace Ecommerce.MonitoringApp.Repository.Models;

public sealed class Order
{
    public int Id { get; init; }
    public int ProductId { get; init; }
    public int Quantity { get; init; }
    public decimal Total { get; init; }
    public DateTime CreatedAt { get; init; }
}
