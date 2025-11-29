using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Models;

[Table("categories")]
[Index("Name", Name = "categories_name_key", IsUnique = true)]
public partial class Category
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(55)]
    public string Name { get; set; } = null!;

    [InverseProperty("Category")]
    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
}
