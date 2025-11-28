using MarketPlace.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketPlace.DTO
{
    public class OrderDTO
    {
        public long Id { get; set; }
        public Guid UserId { get; set; }
        public Guid JobId { get; set; }
        public string Status { get; set; } = null!;
        public DateTime OrderedAt { get; set; }
        public virtual User User { get; set; } = null!;
    }
}
