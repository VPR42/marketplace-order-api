namespace MarketPlace.DTO
{
    public class DecideOrderRequest
    {
        // true  — COMPLETED
        // false — REJECTED
        public bool IsCompleted { get; set; }
    }
}