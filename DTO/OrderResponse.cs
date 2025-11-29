namespace MarketPlace.DTO
{
    public class OrderResponse
    {
        public long Id { get; init; }
        public Guid UserId { get; init; }
        public Guid JobId { get; init; }
        public string Status { get; init; } = null!;
        public DateTime OrderedAt { get; init; }
        public DateTime? StatusChangedAt { get; init; }
    }
}