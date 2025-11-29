using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Models;

[Table("users")]
[Index("Email", Name = "users_email_key", IsUnique = true)]
public partial class User
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("email")]
    [StringLength(100)]
    public string Email { get; set; } = null!;

    [Column("password")]
    [StringLength(255)]
    public string Password { get; set; } = null!;

    [Column("surname")]
    [StringLength(100)]
    public string Surname { get; set; } = null!;

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("patronymic")]
    [StringLength(100)]
    public string Patronymic { get; set; } = null!;

    [Column("avatar_path")]
    public string AvatarPath { get; set; } = null!;

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }

    [Column("city")]
    public int City { get; set; }

    [ForeignKey("City")]
    [InverseProperty("Users")]
    public virtual City CityNavigation { get; set; } = null!;

    [InverseProperty("User")]
    public virtual ICollection<Favourite> Favourites { get; set; } = new List<Favourite>();

    [InverseProperty("Master")]
    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();

    [InverseProperty("User")]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
