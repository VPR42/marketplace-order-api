using System.Globalization;

namespace MarketPlace.Services;

public class OrderStatusService : IOrderStatusService
{
    private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "created",
        "paid",
        "completed",
        "cancelled"
    };

    private static readonly Dictionary<string, string[]> AllowedTransitions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["created"] = new[] { "paid", "cancelled" },
            ["paid"] = new[] { "completed" },
            ["completed"] = Array.Empty<string>(),
            ["cancelled"] = Array.Empty<string>()
        };

    public bool IsValidStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return false;

        return ValidStatuses.Contains(Normalize(status));
    }

    public bool CanTransition(string currentStatus, string newStatus)
    {
        if (!IsValidStatus(currentStatus) || !IsValidStatus(newStatus))
            return false;

        var current = Normalize(currentStatus);
        var target = Normalize(newStatus);

        if (current == target) // если совпадают то норм
            return true;

        if (!AllowedTransitions.TryGetValue(current, out var allowedTargets))
            return false;

        return allowedTargets.Any(s => Normalize(s) == target);
    }

    private static string Normalize(string status) =>
        status.Trim().ToLower(CultureInfo.InvariantCulture);
}