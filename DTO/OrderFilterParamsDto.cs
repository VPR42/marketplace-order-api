using MarketPlace.Models;

namespace MarketPlace.DTO
{
    public class OrderFilterParamsDto
    {
        // Фильтр по статусу (New, Completed, Rejected, etc.)
        public string? Status { get; set; }

        // Фильтр по названию услуги (Job.Name LIKE)
        public string? Search { get; set; }

        // НОВОЕ ПОЛЕ: ID категории для фильтрации
        public int? CategoryId { get; set; } // int? чтобы было необязательным

        // Пагинация
        public int PageNumber { get; set; } = 0;
        public int PageSize { get; set; } = 10;

        // Флаг на то, чьи заказы мы выводим
        public bool IsMasterOrder { get; set; } = false;

    }
}