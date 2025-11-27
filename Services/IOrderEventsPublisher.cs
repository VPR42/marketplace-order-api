using MarketPlace.Models;

namespace MarketPlace.Services
{
    public interface IOrderEventsPublisher
    {
        Task PublishOrderClosedAsync(Order order);
    }
}
