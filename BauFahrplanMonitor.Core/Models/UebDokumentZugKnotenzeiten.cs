using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("ueb_dokument_zug_knotenzeiten", Schema = "ujbaudb")]
[Index("UebDokumentZugRef", "Lfdnr", Name = "ueb_dokument_zug_knotenzeiten_uq_zug_lfdnr", IsUnique = true)]
public partial class UebDokumentZugKnotenzeiten
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("ueb_dokument_zug_ref")]
    public long UebDokumentZugRef { get; set; }

    [Column("lfdnr")]
    public long Lfdnr { get; set; }

    [Column("bst_ref")]
    public long? BstRef { get; set; }

    [Column("ankunft", TypeName = "timestamp without time zone")]
    public DateTime? Ankunft { get; set; }

    [Column("abfahrt", TypeName = "timestamp without time zone")]
    public DateTime? Abfahrt { get; set; }

    [Column("haltart", TypeName = "character varying")]
    public string? Haltart { get; set; }

    [Column("bemerkung", TypeName = "character varying")]
    public string? Bemerkung { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [Column("relativlage")]
    public long? Relativlage { get; set; }

    [ForeignKey("BstRef")]
    [InverseProperty("UebDokumentZugKnotenzeiten")]
    public virtual BasisBetriebsstelle? BstRefNavigation { get; set; }

    [ForeignKey("UebDokumentZugRef")]
    [InverseProperty("UebDokumentZugKnotenzeiten")]
    public virtual UebDokumentZug UebDokumentZugRefNavigation { get; set; } = null!;
}
