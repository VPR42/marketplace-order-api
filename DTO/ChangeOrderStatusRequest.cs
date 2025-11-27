using MarketPlace.Models;

namespace MarketPlace.DTO
{
    // PUT /api/orders/{id}/status
    public class ChangeOrderStatusRequest
    {
        public OrderStatus Status { get; set; }
    }
}