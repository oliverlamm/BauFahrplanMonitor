using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("ujbau_vorgang_bbmn", Schema = "ujbaudb")]
[Index("UjVorgangRef", "Bbmn", Name = "ujbau_vorgang_bbmn_uq_vorgang_bbmn", IsUnique = true)]
public partial class UjbauVorgangBbmn
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("uj_vorgang_ref")]
    public long UjVorgangRef { get; set; }

    [Column("bbmn", TypeName = "character varying")]
    public string Bbmn { get; set; } = null!;

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("UjVorgangRef")]
    [InverseProperty("UjbauVorgangBbmn")]
    public virtual UjbauVorgang UjVorgangRefNavigation { get; set; } = null!;
}
