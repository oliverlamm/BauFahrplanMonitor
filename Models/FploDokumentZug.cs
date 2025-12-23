using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("fplo_dokument_zug", Schema = "ujbaudb")]
[Index("KundeRef", Name = "idx_fplo_dokument_zug_kunde_ref")]
[Index("RegelwegAbBstRef", Name = "idx_fplo_dokument_zug_regelweg_ab_bst_ref")]
[Index("RegelwegZielBstRef", Name = "idx_fplo_dokument_zug_regelweg_ziel_bst_ref")]
[Index("FploDokumentRef", "Zugnr", "Verkehrstag", Name = "idx_fplo_dokzug_dok_zug_tag")]
[Index("FploDokumentRef", "Verkehrstag", "Zugnr", Name = "uq_fplo_zug_dok_tag_zugnr", IsUnique = true)]
public partial class FploDokumentZug
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("fplo_dokument_ref")]
    public long FploDokumentRef { get; set; }

    [Column("bedarf")]
    public bool? Bedarf { get; set; }

    [Column("verkehrstag")]
    public DateOnly Verkehrstag { get; set; }

    [Column("zugnr")]
    public long Zugnr { get; set; }

    [Column("zugbez", TypeName = "character varying")]
    public string? Zugbez { get; set; }

    [Column("zuggat", TypeName = "character varying")]
    public string? Zuggat { get; set; }

    [Column("kunde_ref")]
    public long KundeRef { get; set; }

    [Column("sicherheitsrelevant")]
    public bool? Sicherheitsrelevant { get; set; }

    [Column("lauterzug")]
    public bool? Lauterzug { get; set; }

    [Column("vmax")]
    public long? Vmax { get; set; }

    [Column("tfz", TypeName = "character varying")]
    public string? Tfz { get; set; }

    [Column("last")]
    public long? Last { get; set; }

    [Column("laenge")]
    public long? Laenge { get; set; }

    [Column("brems", TypeName = "character varying")]
    public string? Brems { get; set; }

    [Column("ebula")]
    public bool? Ebula { get; set; }

    [Column("skl", TypeName = "character varying")]
    public string? Skl { get; set; }

    [Column("regelweg_ab_bst_ref")]
    public long RegelwegAbBstRef { get; set; }

    [Column("regelweg_ziel_bst_ref")]
    public long RegelwegZielBstRef { get; set; }

    [Column("regelweg_linie", TypeName = "character varying")]
    public string? RegelwegLinie { get; set; }

    [Column("bemerkung")]
    public string? Bemerkung { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [Column("klv", TypeName = "character varying")]
    public string? Klv { get; set; }

    [ForeignKey("FploDokumentRef")]
    [InverseProperty("FploDokumentZug")]
    public virtual FploDokument FploDokumentRefNavigation { get; set; } = null!;

    [InverseProperty("FploDokumentZugRefNavigation")]
    public virtual ICollection<FploDokumentZugFahrplan> FploDokumentZugFahrplan { get; set; } = new List<FploDokumentZugFahrplan>();

    [InverseProperty("FploDokumentZugRefNavigation")]
    public virtual ICollection<FploDokumentZugRegelung> FploDokumentZugRegelung { get; set; } = new List<FploDokumentZugRegelung>();

    [ForeignKey("KundeRef")]
    [InverseProperty("FploDokumentZug")]
    public virtual BasisKunde KundeRefNavigation { get; set; } = null!;

    [ForeignKey("RegelwegAbBstRef")]
    [InverseProperty("FploDokumentZugRegelwegAbBstRefNavigation")]
    public virtual BasisBetriebsstelle RegelwegAbBstRefNavigation { get; set; } = null!;

    [ForeignKey("RegelwegZielBstRef")]
    [InverseProperty("FploDokumentZugRegelwegZielBstRefNavigation")]
    public virtual BasisBetriebsstelle RegelwegZielBstRefNavigation { get; set; } = null!;
}
