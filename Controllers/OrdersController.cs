using MarketPlace.Data;
using MarketPlace.DTO;
using MarketPlace.Models;
using MarketPlace.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MarketPlace.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IOrderStatusService _orderStatusService;

    public OrdersController(ApplicationDbContext dbContext, IOrderStatusService orderStatusService)
    {
        _dbContext = dbContext;
        _orderStatusService = orderStatusService;
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
            Status = OrderStatus.CREATED.ToString(),
            OrderedAt = DateTime.UtcNow,
            StatusChangedAt = DateTime.UtcNow
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

    // PUT /api/orders/{id}/status
    [HttpPut("{id:long}/status")]
    public async Task<ActionResult<Order>> ChangeStatus(
        long id,
        [FromBody] ChangeOrderStatusRequest request)
    {
        var order = await _dbContext.Orders.FindAsync(id);
        if (order == null) return NotFound();

        var currentStatus = order.Status;
        var newStatus = (request.Status).ToString();

        if (!_orderStatusService.IsValidStatus(newStatus))
            return BadRequest($"Status '{newStatus}' is invalid.");

        if (!_orderStatusService.CanTransition(currentStatus, newStatus))
            return BadRequest($"Transition from '{currentStatus}' to '{newStatus}' is not allowed.");

        if (!string.Equals(currentStatus, newStatus.ToUpperInvariant(), StringComparison.OrdinalIgnoreCase))
        {
            order.Status = newStatus.ToUpperInvariant();
            order.StatusChangedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();
        return order;
    }

    [HttpGet]
    [Authorize]
    [Route("test")]
    public ActionResult<string?> Test()
    {
        var id = HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        return id;
    }
}