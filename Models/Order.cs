using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketPlace.Models;

[Table("orders")]
public partial class Order
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("job_id")]
    public Guid JobId { get; set; }

    [ForeignKey("JobId")]
    [InverseProperty("Orders")]
    public virtual Job Job { get; set; } = null!;

    [Column("status")]
    [StringLength(15)]
    public string Status { get; set; } = null!;

    [Column("ordered_at")]
    public DateTime OrderedAt { get; set; }

    [Column("status_changed_at")]
    public DateTime? StatusChangedAt { get; set; }

    [ForeignKey("JobId")]
    [InverseProperty("Orders")]
    public virtual Job Job { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("Orders")]
    public virtual User User { get; set; } = null!;
}
