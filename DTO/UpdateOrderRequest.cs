namespace MarketPlace.DTO
{
    public class UpdateOrderRequest
    {
        public Guid JobId { get; set; }
        // Сделано через job на которую изменится нынешний заказ
    }
}
