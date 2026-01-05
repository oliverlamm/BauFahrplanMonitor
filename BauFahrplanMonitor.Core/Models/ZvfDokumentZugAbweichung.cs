using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("zvf_dokument_zug_abweichung", Schema = "ujbaudb")]
[Index("ZvfDokumentZugRef", "Art", "AbBstRef", Name = "zvf_dokument_zug_abweichung_uq_zug_art", IsUnique = true)]
public partial class ZvfDokumentZugAbweichung
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("zvf_dokument_zug_ref")]
    public long ZvfDokumentZugRef { get; set; }

    [Column("abweichung", TypeName = "jsonb")]
    public string? Abweichung { get; set; }

    [Column("art", TypeName = "character varying")]
    public string Art { get; set; } = null!;

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [Column("ab_bst_ref")]
    public long? AbBstRef { get; set; }

    [ForeignKey("AbBstRef")]
    [InverseProperty("ZvfDokumentZugAbweichung")]
    public virtual BasisBetriebsstelle? AbBstRefNavigation { get; set; }

    [ForeignKey("ZvfDokumentZugRef")]
    [InverseProperty("ZvfDokumentZugAbweichung")]
    public virtual ZvfDokumentZug ZvfDokumentZugRefNavigation { get; set; } = null!;
}
