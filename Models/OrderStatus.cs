namespace MarketPlace.Models;

[AttributeUsage(AttributeTargets.Field)]
public class StatusInfoAttribute : Attribute
{
    public int Id { get; }
    public string DisplayName { get; }

    public StatusInfoAttribute(int id, string displayName)
    {
        Id = id;
        DisplayName = displayName;
    }
}

public enum OrderStatus
{
    [StatusInfo(0, "Создан")]
    CREATED,

    [StatusInfo(1, "В работе")]
    WORKING,

    [StatusInfo(2, "Завершен")]
    COMPLETED,

    [StatusInfo(-1, "Отменен")]
    CANCELLED,

    [StatusInfo(-2, "Не выполнен")]
    REJECTED
}