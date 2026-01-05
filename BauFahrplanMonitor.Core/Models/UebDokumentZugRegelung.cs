using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("ueb_dokument_zug_regelung", Schema = "ujbaudb")]
[Index("AnkerBstRef", Name = "idx_ueb_dokument_zug_regelung_anker_bst_ref")]
[Index("UebDokumentZugRef", "Art", "AnkerBstRef", Name = "uq_ueb_zreg_zug_art_bst", IsUnique = true)]
public partial class UebDokumentZugRegelung
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("ueb_dokument_zug_ref")]
    public long UebDokumentZugRef { get; set; }

    [Column("regelung", TypeName = "jsonb")]
    public string? Regelung { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [Column("art", TypeName = "character varying")]
    public string Art { get; set; } = null!;

    [Column("anker_bst_ref")]
    public long AnkerBstRef { get; set; }

    [ForeignKey("AnkerBstRef")]
    [InverseProperty("UebDokumentZugRegelung")]
    public virtual BasisBetriebsstelle AnkerBstRefNavigation { get; set; } = null!;

    [ForeignKey("UebDokumentZugRef")]
    [InverseProperty("UebDokumentZugRegelung")]
    public virtual UebDokumentZug UebDokumentZugRefNavigation { get; set; } = null!;
}
