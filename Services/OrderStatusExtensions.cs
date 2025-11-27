using System.Reflection;
using MarketPlace.Models;

namespace MarketPlace.Services;

public static class OrderStatusExtensions
{
    public static StatusInfoAttribute? GetInfo(this OrderStatus status)
    {
        var member = typeof(OrderStatus).GetMember(status.ToString()).First();
        return member.GetCustomAttribute<StatusInfoAttribute>();
    }

    public static int GetId(this OrderStatus status) => status.GetInfo()?.Id ?? 0;

    public static string GetDisplayName(this OrderStatus status) => status.GetInfo()?.DisplayName ?? status.ToString();

    public static bool TryParse(string value, out OrderStatus status)
    {
        return Enum.TryParse(value, ignoreCase: true, out status);
    }
}