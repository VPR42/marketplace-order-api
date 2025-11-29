namespace MarketPlace.DTO
{
    public class OrderResponse
    {
        public long Id { get; init; }
        public string Status { get; init; } = null!;
        public DateTime OrderedAt { get; init; }
        public DateTime? StatusChangedAt { get; init; }
        public UserDto User { get; init; } = null!;
        public JobDto Job { get; init; } = null!;
    }
}