using System;
using System.Collections.Generic;
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

    [Column("working_hours")]
    [StringLength(25)]
    public string? WorkingHours { get; set; }

    [ForeignKey("MasterId")]
    public virtual User Master { get; set; } = null!;
}
