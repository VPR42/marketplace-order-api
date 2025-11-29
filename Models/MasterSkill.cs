using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Models;

[Keyless]
[Table("master_skills")]
public partial class MasterSkill
{
    [Column("master_id")]
    public Guid MasterId { get; set; }

    [Column("skill_id")]
    public int SkillId { get; set; }

    [ForeignKey("MasterId")]
    public virtual User Master { get; set; } = null!;

    [ForeignKey("SkillId")]
    public virtual Skill Skill { get; set; } = null!;
}
