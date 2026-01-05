using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("ujbau_vorgang", Schema = "ujbaudb")]
[Index("VorgangNr", Name = "idx_vorg_vorgang_nr")]
[Index("VorgangNr", "Fahrplanjahr", Name = "ujbau_vorgang_uq_vorgang_nr_fahrplanjahr", IsUnique = true)]
public partial class UjbauVorgang
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("vorgang_nr")]
    public long? VorgangNr { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [Column("kategorie", TypeName = "character varying")]
    public string Kategorie { get; set; } = null!;

    [Column("extension", TypeName = "character varying")]
    public string? Extension { get; set; }

    [Column("ist_qs")]
    public bool? IstQs { get; set; }

    [Column("ist_ks")]
    public bool? IstKs { get; set; }

    [Column("kigbau", TypeName = "character varying")]
    public string? Kigbau { get; set; }

    [Column("korridor", TypeName = "character varying")]
    public string? Korridor { get; set; }

    [Column("fahrplanjahr")]
    public long? Fahrplanjahr { get; set; }

    [InverseProperty("UjbauVorgangRefNavigation")]
    public virtual ICollection<FploDokument> FploDokument { get; set; } = new List<FploDokument>();

    [InverseProperty("UjbauVorgangRefNavigation")]
    public virtual ICollection<UebDokument> UebDokument { get; set; } = new List<UebDokument>();

    [InverseProperty("UjVorgangRefNavigation")]
    public virtual ICollection<UjbauVorgangBbmn> UjbauVorgangBbmn { get; set; } = new List<UjbauVorgangBbmn>();

    [InverseProperty("UjbauVorgangRefNavigation")]
    public virtual ICollection<ZvfDokument> ZvfDokument { get; set; } = new List<ZvfDokument>();
}
