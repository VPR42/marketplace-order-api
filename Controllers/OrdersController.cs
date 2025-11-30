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
    [HttpGet] // Используем базовый GET-путь
    //[Authorize] // Оставляем, т.к. токен нужен
    public async Task<IActionResult> GetPagedOrdersForUser([FromQuery] OrderFilterParamsDto filterParams)
    {
        // 1. Извлечение ID пользователя из токена
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        // Добавим проверку почты, как в рабочем методе, на всякий случай
        var userEmail = User.FindFirstValue(ClaimTypes.Email);

        // 2. ИСПОЛЬЗУЕМ ПАТТЕРН ИЗ РАБОТАЮЩЕГО ЭНДПОИНТА
        if (string.IsNullOrEmpty(userIdString) || string.IsNullOrEmpty(userEmail))
        {
            // Явно возвращаем 401 Unauthorized
            return Unauthorized("User ID not found in token or token is missing.");
        }

        // 3. Остальная логика (как было в предыдущем ответе)

        if (!Guid.TryParse(userIdString, out var userId))
        {
            // Не смогли распарсить ID
            return Unauthorized("Invalid user ID format in token.");
        }

        var (orders, totalCount) = await _orderService.GetPagedUserOrdersAsync(userId, filterParams);

        // 4. Формирование ответа с метаданными для пагинации
        var response = new
        {
            TotalCount = totalCount,
            PageNumber = filterParams.PageNumber,
            PageSize = filterParams.PageSize,
            Orders = orders
        };

        return Ok(response);
    }


    /// <summary>
    /// ВРЕМЕННЫЙ МЕТОД ДЛЯ ТЕСТИРОВАНИЯ БЕЗ АВТОРИЗАЦИИ.
    /// Игнорирует ClaimsPrincipal и использует хардкод ID пользователя.
    /// </summary>
    // Если на контроллере или глобально есть [Authorize], используйте [AllowAnonymous].
    [HttpGet("test-paged-orders")]
    public async Task<IActionResult> GetTestPagedOrders([FromQuery] OrderFilterParamsDto filterParams)
    {
        // ЖЕСТКО ЗАДАННЫЙ ID ПОЛЬЗОВАТЕЛЯ для тестирования (Иван Запара)
        var testUserId = new Guid("324c360f-c890-4def-be92-4a483ee0373d");

        // Здесь мы полностью пропускаем все проверки User.FindFirstValue и Unauthorized()

        // 1. Вызов сервиса с хардкод ID
        var (orders, totalCount) = await _orderService.GetPagedUserOrdersAsync(testUserId, filterParams);

        // 2. Формирование ответа с метаданными для пагинации
        var response = new
        {
            TotalCount = totalCount,
            PageNumber = filterParams.PageNumber,
            PageSize = filterParams.PageSize,
            Orders = orders
        };

        return Ok(response);
    }
}