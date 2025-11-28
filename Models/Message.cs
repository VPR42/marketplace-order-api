using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Models;

[Table("messages")]
public partial class Message
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("master_id")]
    public Guid? MasterId { get; set; }

    [Column("customer_id")]
    public Guid? CustomerId { get; set; }

    [Column("message")]
    [StringLength(1000)]
    public string Message1 { get; set; } = null!;

    [Column("initiator")]
    [StringLength(1)]
    public string Initiator { get; set; } = null!;

    [Column("sent_at", TypeName = "timestamp without time zone")]
    public DateTime SentAt { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("MessageCustomers")]
    public virtual User? Customer { get; set; }

    [ForeignKey("MasterId")]
    [InverseProperty("MessageMasters")]
    public virtual User? Master { get; set; }
}
