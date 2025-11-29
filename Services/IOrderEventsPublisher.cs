using MarketPlace.Models;

namespace MarketPlace.Services
{
    public interface IOrderEventsPublisher
    {
        Task PublishOrderCreatedAsync(Order order);
        Task PublishOrderClosedAsync(Order order);
    }
}
