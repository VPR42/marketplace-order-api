using MarketPlace.Data;
using MarketPlace.DTO;
using MarketPlace.Models;
using MarketPlace.Mappers;
using MarketPlace.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Swashbuckle.AspNetCore.Annotations;

namespace MarketPlace.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IOrderStatusService _orderStatusService;
    private readonly OrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(ApplicationDbContext dbContext, 
        IOrderStatusService orderStatusService, 
        OrderService orderService, ILogger<OrdersController> logger)
    {
        _dbContext = dbContext;
        _orderStatusService = orderStatusService;
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Создает новый заказ.
    /// </summary>
    /// <param name="request">Данные для создания заказа (идентификаторы пользователя и вакансии).</param>
    /// <response code="200">Заказ успешно создан. В теле ответа — созданный заказ.</response>
    /// <response code="400">Пользователь не найден или переданы некорректные данные.</response>
    [HttpPost("create-order")]
    [SwaggerOperation(
        Summary = "Создать заказ",
        Description = "Создает заказ.")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Order>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        Guid userId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!.Value);
        var order = new Order
        {
            UserId = userId,
            JobId = request.JobId,
            Status = OrderStatus.CREATED.ToString(),
            OrderedAt = DateTime.UtcNow,
            StatusChangedAt = DateTime.UtcNow
        };

        _dbContext.Orders.Add(order);
        
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occured creating new order. Reason: {}", ex.Message);
            
            return BadRequest();
        }


        return Ok(order);
    }

    /// <summary>
    /// Изменяет статус заказа.
    /// </summary>
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
            "Обновляет статус заказа. ")]
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
        var newStatus = request.Status.ToString().ToUpperInvariant();

        if (!_orderStatusService.IsValidStatus(newStatus)) return BadRequest($"Status '{newStatus}' is invalid.");
        if (!_orderStatusService.CanTransition(currentStatus, newStatus)) return BadRequest($"Transition from '{currentStatus}' to '{newStatus}' is not allowed.");

        var closingStatuses = new[] { OrderStatus.COMPLETED, OrderStatus.REJECTED };
        var newStatusEnum = request.Status;
        var wasClosing = closingStatuses.Contains(newStatusEnum);

        if (!string.Equals(currentStatus, newStatus, StringComparison.OrdinalIgnoreCase))
        {
            order.Status = newStatus;
            order.StatusChangedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();

        return order;
    }

    /// <summary>
    /// Метод возвращает список заказов текущего пользователя с фильтрами и пагинацией.
    /// Если передавать в поле статус заказа = null, то будут по умолчанию выводить все завершенные и отмененный заказы.
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
            .Include(o => o.User)
                .ThenInclude(u => u.CityNavigation)
            .Include(o => o.Job)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
            return NotFound();

        var response = order.ToOrderResponse();

        return Ok(response);
    }

    // РЕВЬЮ: Временный метод [HttpGet("test-paged-orders")] полностью удален.
}