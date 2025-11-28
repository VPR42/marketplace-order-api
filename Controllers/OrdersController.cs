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

    /// <summary>
    /// Создаёт новый заказ.
    /// </summary>
    /// <remarks>
    /// Пример запроса:
    ///
    ///     POST /api/orders
    ///     {
    ///         "userId": "9fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///         "jobId":  "2c2a4e21-9c00-4a59-9d38-6756b1f005f3"
    ///     }
    ///
    /// Требования:
    /// - Пользователь с указанным <c>UserId</c> должен существовать.
    /// - Заказ создаётся со статусом <c>CREATED</c>.
    /// - Поле <c>StatusChangedAt</c> автоматически устанавливается.
    /// </remarks>
    /// <param name="request">Данные для создания заказа.</param>
    /// <returns>Созданный заказ и ссылка на эндпоинт его получения.</returns>
    /// <response code="201">Заказ успешно создан</response>
    /// <response code="400">Пользователь не существует или запрос некорректен</response>
    [HttpPost]
    [ProducesResponseType(typeof(Order), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

    /// <summary>
    /// Изменяет статус заказа.
    /// </summary>
    /// <remarks>
    /// Пример запроса:
    ///
    ///     PUT /api/orders/12/status
    ///     {
    ///         "status": "WORKING"
    ///     }
    ///
    /// Правила переходов описаны в <c>OrderStatusService</c>.
    /// </remarks>
    /// <param name="id">Идентификатор заказа.</param>
    /// <param name="request">Новый статус заказа.</param>
    /// <returns>Обновлённый заказ.</returns>
    /// <response code="200">Статус успешно изменён</response>
    /// <response code="400">Некорректный статус или переход невозможен</response>
    /// <response code="404">Заказ не найден</response>
    [HttpPut("{id:long}/status")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Order>> ChangeStatus(long id, [FromBody] ChangeOrderStatusRequest request)
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

    /// <summary>
    /// Получает заказ по его идентификатору.
    /// </summary>
    /// <remarks>
    /// Пример запроса:
    ///
    ///     GET /api/orders/12
    ///
    /// </remarks>
    /// <param name="id">Идентификатор заказа.</param>
    /// <returns>Информация о заказе.</returns>
    /// <response code="200">Заказ найден</response>
    /// <response code="404">Заказ не найден</response>
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
    /// Завершает или отклоняет заказ.
    /// </summary>
    /// <remarks>
    /// Пример запроса:
    ///
    ///     POST /api/orders/12/decision
    ///     {
    ///         "isCompleted": true   // завершить
    ///     }
    ///
    /// Или:
    ///
    ///     {
    ///         "isCompleted": false  // отклонить
    ///     }
    ///
    /// Допустимые конечные статусы:
    /// - COMPLETED
    /// - REJECTED
    /// </remarks>
    /// <param name="id">Идентификатор заказа.</param>
    /// <param name="request">Флаг завершения или отклонения.</param>
    /// <returns>Обновлённый заказ.</returns>
    /// <response code="200">Статус изменён</response>
    /// <response code="400">Переход невозможен</response>
    /// <response code="404">Заказ не найден</response>
    [HttpPost("{id:long}/decision")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Order>> Decide(long id, [FromBody] DecideOrderRequest request)
    {
        var order = await _dbContext.Orders.FindAsync(id);
        if (order is null) return NotFound();

        var currentStatus = order.Status;
        var targetStatus = (request.IsCompleted ? OrderStatus.COMPLETED : OrderStatus.REJECTED).ToString();

        if (!_orderStatusService.CanTransition(currentStatus, targetStatus)) return BadRequest($"Transition from '{currentStatus}' to '{targetStatus}' is not allowed."); // проверка на допустимость
        if (string.Equals(currentStatus, targetStatus, StringComparison.OrdinalIgnoreCase)) return order; // вдруг статус уже такой

        order.Status = targetStatus;
        order.StatusChangedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return order;
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