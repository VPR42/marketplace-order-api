using MarketPlace.Data;
using MarketPlace.DTO;
using MarketPlace.Enums;
using MarketPlace.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Services;

public class OrderService
{
    private readonly ApplicationDbContext _dbContext;

    public OrderService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<OrderResponse>> GetLastOrdersForUser(Guid userId)
    {
        var statuses = new[]
        {
            OrderStatus.COMPLETED.ToString(),
            OrderStatus.REJECTED.ToString()
        };

        return await _dbContext.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId && statuses.Contains(o.Status))
            .Include(o => o.User)
                .ThenInclude(u => u.CityNavigation)
            .Include(o => o.Job) 
            .OrderByDescending(o => o.OrderedAt)
            .Take(5)
            .Select(o => new OrderResponse
            {
                Id = o.Id,
                Status = o.Status,
                OrderedAt = o.OrderedAt,
                StatusChangedAt = o.StatusChangedAt,

                User = new UserDto
                {
                    Id = o.User.Id,
                    Surname = o.User.Surname,
                    Name = o.User.Name,
                    Patronymic = o.User.Patronymic,
                    Email = o.User.Email,
                    AvatarPath = o.User.AvatarPath,
                    City = new CityDto
                    {
                        Id = o.User.CityNavigation.Id,
                        Name = o.User.CityNavigation.Name,
                        Region = o.User.CityNavigation.Region
                    }
                },

                Job = new JobDto
                {
                    Id = o.Job.Id,
                    Name = o.Job.Name,
                    Description = o.Job.Description,
                    Price = o.Job.Price,
                    CoverUrl = o.Job.CoverUrl,
                    CategoryId = o.Job.CategoryId
                }
            })
            .ToListAsync();
    }
}
