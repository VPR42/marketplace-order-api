using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Models;

[Table("tags")]
[Index("Name", Name = "tags_name_key", IsUnique = true)]
public partial class Tag
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(55)]
    public string Name { get; set; } = null!;

    [ForeignKey("TagId")]
    [InverseProperty("Tags")]
    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
}
