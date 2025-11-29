namespace MarketPlace.DTO
{
    public class JobDto
    {
        public Guid Id { get; init; }

        public string Name { get; init; } = null!;
        public string Description { get; init; } = null!;
        public decimal Price { get; init; }

        public string? CoverUrl { get; init; }

        public DateTime CreatedAt { get; init; }

        public UserDto User { get; init; } = null!;

        public int CategoryId { get; init; }

        public CategoryDto Category { get; init; } = null!;

        public List<TagDto> Tags { get; init; } = new();

        public long OrdersCount { get; init; }
    }
}