using MarketPlace.Models;

namespace MarketPlace.DTO
{
    public class UserOrderListDto
    {
        // Из таблицы Order
        public long OrderId { get; set; }
        public string Status { get; set; }
        public DateTime OrderedAt { get; set; }

        // Из таблицы Job
        public string JobName { get; set; }
        public string JobDescription { get; set; }
        public decimal JobPrice { get; set; }
        public string? JobCoverUrl { get; set; }

        // Из Job.Category
        public string CategoryName { get; set; }

        // Из Job.Master (Master/User)
        public string MasterFullName { get; set; } // Объединенное имя и фамилия мастера
        public int MasterCityId { get; set; } // Можно вернуть ID города, если потребуется город.
    }
}