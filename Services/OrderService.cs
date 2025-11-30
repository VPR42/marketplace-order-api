using MarketPlace.Data;
using MarketPlace.DTO;
using MarketPlace.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq; // Добавлен для IQueryable

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


        /// <summary>
        /// Выводит заказы пользователя с фильтрацией и пагинацией.
        /// </summary>
        /// <returns>Кортеж с отфильтрованными DTO и общим количеством записей.</returns>
        public async Task<(IEnumerable<UserOrderListDto> orders, int totalCount)> GetPagedUserOrdersAsync(
            Guid userId,
            OrderFilterParamsDto filterParams)
        {
            // Начинаем запрос с Order. 
            // 💡 Оптимизация: .Include() можно удалить, если вся выборка делается через .Select() (проецирование).
            // EF Core автоматически добавит JOIN'ы в SQL, необходимые для .Select().
            var query = _dbContext.Orders
                // Фильтруем по ID текущего пользователя. Это ключевой элемент безопасности!
                .Where(o => o.UserId == userId)
                .AsQueryable(); // Важно, чтобы это был IQueryable для построения запроса

            // --- 1. ПРИМЕНЕНИЕ ФИЛЬТРОВ ---

            // Фильтр по статусу (например, Status=COMPLETED). Проверяем на пустую строку, а не на 'ВСЕ'.
            if (!string.IsNullOrWhiteSpace(filterParams.Status))
            {
                query = query.Where(o => o.Status == filterParams.Status.ToUpperInvariant());
            }

            // Фильтр по поисковой строке (Job.Name LIKE %search%)
            if (!string.IsNullOrWhiteSpace(filterParams.Search))
            {
                // .Contains() в EF Core транслируется в SQL LIKE '%value%'
                query = query.Where(o => o.Job.Name.Contains(filterParams.Search));
            }

            // НОВЫЙ ФИЛЬТР: ПО КАТЕГОРИИ (используем int?, чтобы проверить на наличие значения)
            if (filterParams.CategoryId.HasValue)
            {
                // EF Core автоматически добавит JOIN к таблице Jobs для проверки CategoryId
                query = query.Where(o => o.Job.CategoryId == filterParams.CategoryId.Value);
            }

            // --- 2. ПАГИНАЦИЯ ---

            // Сначала считаем общее количество для фронтенда (до Skip/Take)
            var totalCount = await query.CountAsync();

            // Сортировка (по умолчанию: самые новые заказы в начале)
            query = query.OrderByDescending(o => o.OrderedAt);

            // Рассчитываем, сколько записей нужно пропустить (Skip)
            var skipCount = filterParams.PageNumber * filterParams.PageSize;

            // Применяем Skip/Take
            var pagedOrders = await query
                .Skip(skipCount)
                .Take(filterParams.PageSize)
                // --- 3. ПРОЕКЦИЯ (SELECT) ---
                // Проецирование (Select) в DTO (UserOrderListDto) должно быть последним шагом
                // для минимизации передаваемых данных.
                .Select(o => new UserOrderListDto
                {
                    OrderId = o.Id,
                    Status = o.Status,
                    OrderedAt = o.OrderedAt,

                    // Job-поля
                    JobName = o.Job.Name,
                    JobDescription = o.Job.Description,
                    JobPrice = o.Job.Price,
                    JobCoverUrl = o.Job.CoverUrl,

                    // Связанные поля
                    CategoryName = o.Job.Category.Name,
                    // Добавим проверку на null, если мастер может быть null (хотя по схеме он должен быть)
                    MasterFullName = o.Job.Master != null ? $"{o.Job.Master.Name} {o.Job.Master.Surname}" : "N/A",
                    MasterCityId = o.Job.Master.City
                })
                .ToListAsync();

            return (pagedOrders, totalCount);
        }

    }
}