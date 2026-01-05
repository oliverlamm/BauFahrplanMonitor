using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("fplo_dokument_zug_regelung", Schema = "ujbaudb")]
[Index("AnkerBstRef", Name = "idx_fplo_dokument_zug_regelung_anker_bst_ref")]
[Index("FploDokumentZugRef", "Art", "AnkerBstRef", Name = "uq_fplo_zreg_zug_art_bst", IsUnique = true)]
public partial class FploDokumentZugRegelung
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("fplo_dokument_zug_ref")]
    public long FploDokumentZugRef { get; set; }

    [Column("regelung", TypeName = "jsonb")]
    public string? Regelung { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [Column("art", TypeName = "character varying")]
    public string Art { get; set; } = null!;

    [Column("anker_bst_ref")]
    public long AnkerBstRef { get; set; }

    [ForeignKey("AnkerBstRef")]
    [InverseProperty("FploDokumentZugRegelung")]
    public virtual BasisBetriebsstelle AnkerBstRefNavigation { get; set; } = null!;

    [ForeignKey("FploDokumentZugRef")]
    [InverseProperty("FploDokumentZugRegelung")]
    public virtual FploDokumentZug FploDokumentZugRefNavigation { get; set; } = null!;
}
