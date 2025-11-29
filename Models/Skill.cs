using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Models;

[Table("skills")]
[Index("Name", Name = "skills_name_key", IsUnique = true)]
public partial class Skill
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(55)]
    public string Name { get; set; } = null!;
}
