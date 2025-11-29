using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketPlace.Models
{
    public class Job
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("master_id")]
        public Guid MasterId { get; set; }

        [Column("name")]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [Column("description")]
        public string Description { get; set; } = null!;

        [Column("price", TypeName = "numeric(8,2)")]
        public decimal Price { get; set; }

        [Column("cover_url", TypeName = "text")]
        public string? CoverUrl { get; set; }

        [Column("category_id")]
        public int CategoryId { get; set; }

        [Column("created_at", TypeName = "timestamp with time zone")]
        public DateTime CreatedAt { get; set; }

        [InverseProperty("Job")]
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
