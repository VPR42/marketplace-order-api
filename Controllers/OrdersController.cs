using MarketPlace.Data;
using MarketPlace.DTO;
using MarketPlace.Models;
using MarketPlace.Services;
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

        order.Status = newStatus.ToUpperInvariant();

        await _dbContext.SaveChangesAsync();
        return order;
    }

    /// <summary>
    /// Метод возвращает последние 5 или меньше заказов для пользователя
    /// </summary>
    /// <returns>JSON Содержащий информацию о заказах (Название заказа, стоимость заказа, когда заказан, и его статус) или Unauthorized</returns>
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


    /// <summary>
    /// Метод возвращает список заказов текущего пользователя с фильтрами и пагинацией.
    /// </summary>
    /// <returns>JSON, содержащий список заказов и информацию о пагинации.</returns>
    // ИЗМЕНЕНИЕ: Указываем возвращаемый тип с использованием новой DTO-обертки
    [HttpGet]
    public async Task<ActionResult<PagedResponseDto<UserOrderListDto>>> GetPagedOrdersForUser([FromQuery] OrderFilterParamsDto filterParams)
    {
        // РЕВЬЮ: Удалены все лишние проверки (string.IsNullOrEmpty, Guid.TryParse),
        // так как Middleware гарантирует наличие валидного ID.

        // 1. Извлечение ID пользователя и парсинг. Если ID невалиден, Guid.Parse() вызовет исключение (500 Internal Server Error), 
        // что соответствует требованию не добавлять явные проверки 401 в контроллер.
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = Guid.Parse(userIdString);

        var (orders, totalCount) = await _orderService.GetPagedUserOrdersAsync(userId, filterParams);

        // 2. Формирование ответа с использованием PagedResponseDto
        var response = new PagedResponseDto<UserOrderListDto>(
            orders,
            totalCount,
            filterParams.PageNumber,
            filterParams.PageSize
        );

        return Ok(response);
    }

    // РЕВЬЮ: Временный метод [HttpGet("test-paged-orders")] полностью удален.
}