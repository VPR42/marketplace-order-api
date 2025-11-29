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
    /// <returns>Информация о заказе с данными пользователя и вакансии.</returns>
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

        var response = new OrderResponse
        {
            Id = order.Id,
            Status = order.Status,
            OrderedAt = order.OrderedAt,
            StatusChangedAt = order.StatusChangedAt,

            User = new UserDto
            {
                Id = order.User.Id,
                Surname = order.User.Surname,
                Name = order.User.Name,
                Patronymic = order.User.Patronymic,
                Email = order.User.Email,
                AvatarPath = order.User.AvatarPath,
                City = new CityDto
                {
                    Id = order.User.CityNavigation.Id,
                    Name = order.User.CityNavigation.Name,
                    Region = order.User.CityNavigation.Region
                }
            },

            Job = new JobDto
            {
                Id = order.Job.Id,
                Name = order.Job.Name,
                Description = order.Job.Description,
                Price = order.Job.Price,
                CoverUrl = order.Job.CoverUrl,
                CategoryId = order.Job.CategoryId
            }
        };

        return Ok(response);
    }

    /// <summary>
    /// Возвращает полную информацию о заказе: данные заказа, пользователя и города.
    /// </summary>
    /// <remarks>
    /// Пример запроса:
    ///
    ///     GET /api/orders/12/details
    ///
    /// </remarks>
    /// <param name="id">Идентификатор заказа.</param>
    /// <returns>Полная информация о заказе.</returns>
    /// <response code="200">Информация о заказе успешно получена</response>
    /// <response code="404">Заказ не найден</response>
    [HttpGet("{id:long}/details")]
    [ProducesResponseType(typeof(OrderDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDetailsResponse>> GetOrderDetails(long id)
    {
        var order = await _dbContext.Orders.AsNoTracking().Include(o => o.User).ThenInclude(u => u.CityNavigation).FirstOrDefaultAsync(o => o.Id == id);

        if (order is null) return NotFound();

        var user = order.User;
        var city = user.CityNavigation;

        var response = new OrderDetailsResponse
        {
            Id = order.Id,

            JobId = order.JobId,
            Status = order.Status,
            OrderedAt = order.OrderedAt,
            StatusChangedAt = order.StatusChangedAt,

            UserId = user.Id,
            UserSurname = user.Surname,
            UserName = user.Name,
            UserPatronymic = user.Patronymic,
            UserEmail = user.Email,
            UserAvatarPath = user.AvatarPath,

            CityId = city.Id,
            CityName = city.Name,
            CityRegion = city.Region
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