using MarketPlace.Data;
using MarketPlace.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Services
{
    public class OrderService
    {
        private readonly ApplicationDbContext _dbContext;
        public OrderService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Order>> GetLastOrdersForUser(Guid userId)
        {
            var statuses = new[] { OrderStatus.COMPLETED.ToString(), OrderStatus.REJECTED.ToString() };

            var orders = await _dbContext.Orders
                .Where(o => o.UserId == userId && statuses.Contains(o.Status))
                .OrderByDescending(o => o.OrderedAt)
                .Take(5)
                .ToListAsync();

            return orders;
        }
    }
}
