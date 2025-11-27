namespace MarketPlace.Services
{
    public interface IOrderStatusService
    {
        bool IsValidStatus(string status);
        bool CanTransition(string currentStatus, string newStatus);
    }
}