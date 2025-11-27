namespace MarketPlace.DTO
{
    // POST /api/orders
    public class CreateOrderRequest
    {
        public Guid UserId { get; set; }
        public Guid JobId { get; set; }
    }
}