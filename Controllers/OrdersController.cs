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
    private readonly OrderService _orderService;

    public OrdersController(ApplicationDbContext dbContext, IOrderStatusService orderStatusService, OrderService orderService)
    {
        _dbContext = dbContext;
        _orderStatusService = orderStatusService;
        _orderService = orderService;
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

        return CreatedAtAction(nameof(GetOrderId), new { id = order.Id }, order);
    }

    // Нужен именно для создания
    private async Task<ActionResult<Order>> GetOrderId(long id)
    {
        var order = await _dbContext.Orders.FindAsync(id);
        if (order == null) return NotFound();

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
    [HttpGet("GetLastOrders")]
    public async Task<IActionResult> GetLastOrders()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userEmail = User.FindFirstValue(ClaimTypes.Email);

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userEmail))
            return Unauthorized();


        var orders = await _orderService.GetLastOrdersForUser(Guid.Parse(userId));
        return Ok(orders);
    }

    // GET /api/orders/{id}
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> GetOrderById(long id)
    {
        var order = await _dbContext.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
            return NotFound();

        var response = new OrderResponse
        {
            Id = order.Id,
            UserId = order.UserId,
            JobId = order.JobId,
            Status = order.Status,
            OrderedAt = order.OrderedAt,
            StatusChangedAt = order.StatusChangedAt
        };

        return Ok(response);
    }

    /// <summary>
    /// Редактирует заказ.
    /// </summary>
    /// <remarks>
    /// Разрешено редактировать только заказы в статусе <c>CREATED</c>.
    ///
    /// Пример запроса:
    ///
    ///     PUT /api/orders/12
    ///     {
    ///         "jobId": "2c2a4e21-9c00-4a59-9d38-6756b1f005f3"
    ///     }
    /// </remarks>
    /// <param name="id">Идентификатор заказа.</param>
    /// <param name="request">Новые параметры заказа.</param>
    /// <returns>Обновлённый заказ.</returns>
    /// <response code="200">Заказ успешно отредактирован</response>
    /// <response code="400">Редактирование невозможно (неподходящий статус или некорректные данные)</response>
    /// <response code="404">Заказ не найден</response>
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Order>> UpdateOrder(
        long id,
        [FromBody] UpdateOrderRequest request)
    {
        var order = await _dbContext.Orders.FindAsync(id);
        if (order is null)
            return NotFound();

        // Разрешено редактировать только когда CREATED
        if (!string.Equals(order.Status, OrderStatus.CREATED.ToString(), StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only orders in status 'CREATED' can be edited.");

        order.JobId = request.JobId;
        order.StatusChangedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(order);
    }

    [HttpGet]
    [Authorize]
    [Route("test")]
    public ActionResult<string?> Test()
    {
        var id = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        return id;
    }
}