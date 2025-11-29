using MarketPlace.Data;
using MarketPlace.DTO;
using MarketPlace.Models;
using MarketPlace.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Swashbuckle.AspNetCore.Annotations;

namespace MarketPlace.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IOrderStatusService _orderStatusService;
    private readonly IOrderEventsPublisher _orderEventsPublisher;
    private readonly OrderService _orderService;

    public OrdersController(ApplicationDbContext dbContext, IOrderStatusService orderStatusService, IOrderEventsPublisher orderEventsPublisher, OrderService orderService)
    {
        _dbContext = dbContext;
        _orderStatusService = orderStatusService;
        _orderEventsPublisher = orderEventsPublisher;
        _orderService = orderService;
    }

    /// <summary>
    /// Создает новый заказ.
    /// </summary>
    /// <remarks>
    /// При успешном создании:
    /// <list type="bullet">
    ///   <item>
    ///     <description>Заказ сохраняется в базе данных.</description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       В RabbitMQ (exchange <c>marketplace.orders</c>) публикуется событие
    ///       с routing key <c>order.created</c>.
    ///     </description>
    ///   </item>
    /// </list>
    /// Пример тела события <c>order.created</c>:
    /// <code>
    /// {
    ///   "type": "order_created",
    ///   "orderId": 123,
    ///   "userId": 10,
    ///   "jobId": 5,
    ///   "status": "CREATED",
    ///   "orderedAt": "2025-11-29T10:15:00Z"
    /// }
    /// </code>
    /// </remarks>
    /// <param name="request">Данные для создания заказа (идентификаторы пользователя и вакансии).</param>
    /// <response code="201">Заказ успешно создан. В теле ответа — созданный заказ.</response>
    /// <response code="400">Пользователь не найден или переданы некорректные данные.</response>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Создать заказ",
        Description = "Создает заказ и публикует событие 'order.created' в RabbitMQ (exchange 'marketplace.orders').")]
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
        await _orderEventsPublisher.PublishOrderCreatedAsync(order);

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
    /// Логика:
    /// <list type="bullet">
    ///   <item>
    ///     <description>Проверяется корректность нового статуса и допустимость перехода.</description>
    ///   </item>
    ///   <item>
    ///     <description>Статус и время изменения статуса обновляются в базе данных.</description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Если новый статус один из <c>COMPLETED</c> или <c>REJECTED</c>,
    ///       в RabbitMQ (exchange <c>marketplace.orders</c>) публикуется событие
    ///       с routing key <c>order.closed</c>.
    ///     </description>
    ///   </item>
    /// </list>
    /// Пример тела события <c>order.closed</c>:
    /// <code>
    /// {
    ///   "type": "order_closed",
    ///   "orderId": 123,
    ///   "userId": 10,
    ///   "jobId": 5,
    ///   "status": "COMPLETED",
    ///   "closedAt": "2025-11-29T11:00:00Z"
    /// }
    /// </code>
    /// </remarks>
    /// <param name="id">Идентификатор заказа.</param>
    /// <param name="request">Новый статус заказа.</param>
    /// <response code="200">
    /// Статус заказа успешно изменен. В теле ответа — обновленный заказ.
    /// </response>
    /// <response code="400">
    /// Некорректный статус или запрещенный переход статусов.
    /// </response>
    /// <response code="404">
    /// Заказ с указанным идентификатором не найден.
    /// </response>
    [HttpPut("{id:long}/status")]
    [SwaggerOperation(
        Summary = "Изменить статус заказа",
        Description =
            "Обновляет статус заказа. При переходе в COMPLETED или REJECTED публикует событие 'order.closed' в RabbitMQ.")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Order>> ChangeStatus(
        long id,
        [FromBody] ChangeOrderStatusRequest request)
    {
        var order = await _dbContext.Orders.FindAsync(id);
        if (order == null) return NotFound();

        var currentStatus = order.Status;
        var newStatus = (request.Status).ToString();

        if (!_orderStatusService.IsValidStatus(newStatus)) return BadRequest($"Status '{newStatus}' is invalid.");
        if (!_orderStatusService.CanTransition(currentStatus, newStatus)) return BadRequest($"Transition from '{currentStatus}' to '{newStatus}' is not allowed.");

        var closingStatuses = new[] { OrderStatus.COMPLETED, OrderStatus.REJECTED };
        var newStatusEnum = request.Status;
        var wasClosing = closingStatuses.Contains(newStatusEnum);

        if (!string.Equals(currentStatus, newStatus.ToUpperInvariant(), StringComparison.OrdinalIgnoreCase))
        {
            order.Status = newStatus.ToUpperInvariant();
            order.StatusChangedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();

        if (wasClosing) await _orderEventsPublisher.PublishOrderClosedAsync(order);

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

    [HttpGet]
    [Authorize]
    [Route("test")]
    public ActionResult<string?> Test()
    {
        var id = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        return id;
    }
}
