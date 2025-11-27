namespace MarketPlace.DTO
{
    // PATCH /api/orders/{id}/status
    public class ChangeOrderStatusRequest
    {
        public string Status { get; set; } = null!;
    }
}