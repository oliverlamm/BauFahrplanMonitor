using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("zvf_dokument_streckenabschnitte", Schema = "ujbaudb")]
[Index("ZvfDokumentRef", "StartBstRl100", "EndBstRl100", "Massnahme", "Betriebsweise", "Grund", "Baubeginn", "Bauende", Name = "zvf_doc_strabs_unique", IsUnique = true)]
public partial class ZvfDokumentStreckenabschnitte
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("zvf_dokument_ref")]
    public long ZvfDokumentRef { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [Column("start_bst_rl100", TypeName = "character varying")]
    public string StartBstRl100 { get; set; } = null!;

    [Column("end_bst_rl100", TypeName = "character varying")]
    public string EndBstRl100 { get; set; } = null!;

    [Column("massnahme", TypeName = "character varying")]
    public string Massnahme { get; set; } = null!;

    [Column("betriebsweise", TypeName = "character varying")]
    public string Betriebsweise { get; set; } = null!;

    [Column("grund", TypeName = "character varying")]
    public string Grund { get; set; } = null!;

    [Column("baubeginn", TypeName = "timestamp without time zone")]
    public DateTime Baubeginn { get; set; }

    [Column("bauende", TypeName = "timestamp without time zone")]
    public DateTime Bauende { get; set; }

    [Column("zeitraum_unterbrochen")]
    public bool? ZeitraumUnterbrochen { get; set; }

    [Column("vzg_strecke", TypeName = "character varying")]
    public string? VzgStrecke { get; set; }

    [ForeignKey("ZvfDokumentRef")]
    [InverseProperty("ZvfDokumentStreckenabschnitte")]
    public virtual ZvfDokument ZvfDokumentRefNavigation { get; set; } = null!;
}
