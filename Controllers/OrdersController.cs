using MarketPlace.Data;
using MarketPlace.DTO;
using MarketPlace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public OrdersController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // POST /api/orders
    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var userExists = await _dbContext.Users.AnyAsync(u => u.Id == request.UserId);

        if (!userExists)
        {
            return BadRequest($"User with ID {request.UserId} does not exist.");
        }

        var order = new Order
        {
            UserId = request.UserId,
            JobId = request.JobId,
            Status = "created",
            OrderedAt = DateTime.UtcNow
        };

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<Order>> GetOrderById(long id)
    {
        var order = await _dbContext.Orders.FindAsync(id);
        if (order == null)
            return NotFound();

        return order;
    }
}