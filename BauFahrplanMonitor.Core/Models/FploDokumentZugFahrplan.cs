using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("fplo_dokument_zug_fahrplan", Schema = "ujbaudb")]
[Index("FploDokumentZugRef", "Lfdnr", Name = "fplo_dokument_zug_fahrplan_uq_zug_lfdnr", IsUnique = true)]
[Index("BstRef", Name = "idx_fplo_dokument_zug_fahrplan_bst_ref")]
public partial class FploDokumentZugFahrplan
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("fplo_dokument_zug_ref")]
    public long FploDokumentZugRef { get; set; }

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

    [Column("strecke", TypeName = "character varying")]
    public string? Strecke { get; set; }

    [Column("bemerkung", TypeName = "character varying")]
    public string? Bemerkung { get; set; }

    [Column("bfpl", TypeName = "jsonb")]
    public string? Bfpl { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("BstRef")]
    [InverseProperty("FploDokumentZugFahrplan")]
    public virtual BasisBetriebsstelle? BstRefNavigation { get; set; }

    [ForeignKey("FploDokumentZugRef")]
    [InverseProperty("FploDokumentZugFahrplan")]
    public virtual FploDokumentZug FploDokumentZugRefNavigation { get; set; } = null!;
}
