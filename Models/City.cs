using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Models;

[Table("cities")]
[Index("Region", "Name", Name = "uq_region_city_name", IsUnique = true)]
public partial class City
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("region")]
    [StringLength(55)]
    public string Region { get; set; } = null!;

    [Column("name")]
    [StringLength(55)]
    public string Name { get; set; } = null!;

    [InverseProperty("CityNavigation")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
