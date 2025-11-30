using MarketPlace.Data;
using MarketPlace.DTO;
using MarketPlace.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq; // Добавлен для IQueryable

namespace MarketPlace.Services
{
    public class OrderService
    {
        private readonly ApplicationDbContext _dbContext;

        public OrderService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Этот метод не требует изменений.
        public async Task<List<LastOrderDTO>> GetLastOrdersForUser(Guid userId)
        {
            var statuses = new[] { OrderStatus.COMPLETED.ToString(), OrderStatus.REJECTED.ToString() };

            var orders = await _dbContext.Orders
                .Where(o => o.UserId == userId && statuses.Contains(o.Status))
                .OrderByDescending(o => o.OrderedAt)
                .Take(5)
                .Select(e => new LastOrderDTO
                {
                    OrderName = e.Job.Name,
                    OrderedAt = e.OrderedAt,
                    OrderPrice = e.Job.Price,
                    Status = e.Status
                })
                .ToListAsync();

            return orders;
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
            var skipCount = (filterParams.PageNumber - 1) * filterParams.PageSize;

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