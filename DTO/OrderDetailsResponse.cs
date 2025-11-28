namespace MarketPlace.DTO;

public class OrderDetailsResponse
{
    public long Id { get; init; }

    // Информация о заказе
    public Guid JobId { get; init; }
    public string Status { get; init; } = null!;
    public DateTime OrderedAt { get; init; }
    public DateTime? StatusChangedAt { get; init; }

    // Информация о пользователе
    public Guid UserId { get; init; }
    public string UserSurname { get; init; } = null!;
    public string UserName { get; init; } = null!;
    public string? UserPatronymic { get; init; }
    public string UserEmail { get; init; } = null!;
    public string UserAvatarPath { get; init; } = null!;

    // Информация о городе пользователя
    public int CityId { get; init; }
    public string CityName { get; init; } = null!;
    public string CityRegion { get; init; } = null!;
}