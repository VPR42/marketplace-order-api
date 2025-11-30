using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Models;

[Keyless]
[Table("masters_info")]
[Index("PhoneNumber", Name = "masters_info_phone_number_key", IsUnique = true)]
[Index("Pseudonym", Name = "masters_info_pseudonym_key", IsUnique = true)]
public partial class MastersInfo
{
    [Column("master_id")]
    public Guid MasterId { get; set; }

    [Column("experience")]
    public int Experience { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("pseudonym")]
    [StringLength(100)]
    public string? Pseudonym { get; set; }

    [Column("phone_number")]
    [StringLength(10)]
    public string PhoneNumber { get; set; } = null!;

    [Column("about")]
    public string? About { get; set; }

    [Column("days_of_week")]
    public List<int>? DaysOfWeek { get; set; }

    [Column("start_time")]
    [StringLength(5)]
    public string? StartTime { get; set; }

    [Column("end_time")]
    [StringLength(5)]
    public string? EndTime { get; set; }

    [ForeignKey("MasterId")]
    public virtual User Master { get; set; } = null!;
}
