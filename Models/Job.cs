using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Models;

[Table("jobs")]
public partial class Job
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

    [Column("price")]
    [Precision(8, 2)]
    public decimal Price { get; set; }

    [Column("cover_url")]
    public string? CoverUrl { get; set; }

    [Column("category_id")]
    public int CategoryId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("Jobs")]
    public virtual Category Category { get; set; } = null!;

    [InverseProperty("Job")]
    public virtual ICollection<Favourite> Favourites { get; set; } = new List<Favourite>();

    [ForeignKey("MasterId")]
    [InverseProperty("Jobs")]
    public virtual User Master { get; set; } = null!;

    [InverseProperty("Job")]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    [ForeignKey("JobId")]
    [InverseProperty("Jobs")]
    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
