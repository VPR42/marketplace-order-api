namespace MarketPlace.DTO
{
    public class JobDto
    {
        public Guid Id { get; init; }

        public string Name { get; init; } = null!;
        public string Description { get; init; } = null!;
        public decimal Price { get; init; }

        public string? CoverUrl { get; init; }

        public int CategoryId { get; init; }
    }
}
