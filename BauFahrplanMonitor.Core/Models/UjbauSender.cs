using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Core.Models;

[Table("ujbau_sender", Schema = "ujbaudb")]
[Index("Name", "Vorname", "Email", Name = "ujbau_sender_uq_name_vorname_email", IsUnique = true)]
public partial class UjbauSender
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("name", TypeName = "character varying")]
    public string? Name { get; set; }

    [Column("vorname", TypeName = "character varying")]
    public string? Vorname { get; set; }

    [Column("kuerzel", TypeName = "character varying")]
    public string? Kuerzel { get; set; }

    [Column("abteilung", TypeName = "character varying")]
    public string? Abteilung { get; set; }

    [Column("strasse", TypeName = "character varying")]
    public string? Strasse { get; set; }

    [Column("plz")]
    public int? Plz { get; set; }

    [Column("stadt", TypeName = "character varying")]
    public string? Stadt { get; set; }

    [Column("email", TypeName = "character varying")]
    public string? Email { get; set; }

    [Column("telefon", TypeName = "character varying")]
    public string? Telefon { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("SenderRefNavigation")]
    public virtual ICollection<FploDokument> FploDokument { get; set; } = new List<FploDokument>();

    [InverseProperty("SenderRefNavigation")]
    public virtual ICollection<UebDokument> UebDokument { get; set; } = new List<UebDokument>();

    [InverseProperty("SenderRefNavigation")]
    public virtual ICollection<ZvfDokument> ZvfDokument { get; set; } = new List<ZvfDokument>();
}
