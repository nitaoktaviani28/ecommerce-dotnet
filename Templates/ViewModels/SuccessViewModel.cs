using Ecommerce.MonitoringApp.Repository.Models;

namespace Ecommerce.MonitoringApp.Templates.ViewModels;

public sealed class SuccessViewModel
{
    public Order Order { get; init; } = null!;
    public Product Product { get; init; } = null!;
}
