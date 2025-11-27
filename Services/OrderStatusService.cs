using MarketPlace.Models;

namespace MarketPlace.Services;

public class OrderStatusService : IOrderStatusService
{
    private static readonly Dictionary<OrderStatus, OrderStatus[]> AllowedTransitions = new()
    {
        [OrderStatus.CREATED] = new[] { OrderStatus.PAID, OrderStatus.CANCELLED, OrderStatus.REJECTED },
        [OrderStatus.PAID] = new[] { OrderStatus.COMPLETED },
        [OrderStatus.COMPLETED] = Array.Empty<OrderStatus>(),
        [OrderStatus.CANCELLED] = Array.Empty<OrderStatus>(),
        [OrderStatus.REJECTED] = Array.Empty<OrderStatus>()
    };

    public bool IsValidStatus(string status)
    {
        return OrderStatusExtensions.TryParse(status, out _);
    }

    public bool CanTransition(string currentStatus, string newStatus)
    {
        if (!OrderStatusExtensions.TryParse(currentStatus, out var current))
            return false;

        if (!OrderStatusExtensions.TryParse(newStatus, out var target))
            return false;

        if (current == target)
            return true;

        return AllowedTransitions[current].Contains(target);
    }
}