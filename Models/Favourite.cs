using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Models;

[PrimaryKey("UserId", "JobId")]
[Table("favourites")]
public partial class Favourite
{
    [Key]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Key]
    [Column("job_id")]
    public Guid JobId { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("JobId")]
    [InverseProperty("Favourites")]
    public virtual Job Job { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("Favourites")]
    public virtual User User { get; set; } = null!;
}
