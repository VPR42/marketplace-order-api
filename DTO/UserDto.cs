namespace MarketPlace.DTO
{
    public class UserDto
    {
        public Guid Id { get; init; }

        public string Surname { get; init; } = null!;
        public string Name { get; init; } = null!;
        public string? Patronymic { get; init; }

        public string Email { get; init; } = null!;
        public string AvatarPath { get; init; } = null!;

        public CityDto City { get; init; } = null!;
    }
}
