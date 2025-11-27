using System.Threading.Tasks;
using MarketPlace.Models;

namespace MarketPlace.Services;

public interface IOrderEventsPublisher
{
    Task PublishOrderCreatedAsync(Order order);
}