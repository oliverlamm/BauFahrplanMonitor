using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("ueb_dokument_zug", Schema = "ujbaudb")]
[Index("KundeRef", Name = "idx_ueb_dokument_zug_kunde_ref")]
[Index("RegelwegAbBstRef", Name = "idx_ueb_dokument_zug_regelweg_ab_bst_ref")]
[Index("RegelwegZielBstRef", Name = "idx_ueb_dokument_zug_regelweg_ziel_bst_ref")]
[Index("UebDokumentRef", "Zugnr", "Verkehrstag", Name = "idx_ueb_dokzug_dok_zug_tag")]
[Index("UebDokumentRef", "Verkehrstag", "Zugnr", Name = "uq_ueb_zug_dok_tag_zugnr", IsUnique = true)]
public partial class UebDokumentZug
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("ueb_dokument_ref")]
    public long UebDokumentRef { get; set; }

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

    [ForeignKey("KundeRef")]
    [InverseProperty("UebDokumentZug")]
    public virtual BasisKunde KundeRefNavigation { get; set; } = null!;

    [ForeignKey("RegelwegAbBstRef")]
    [InverseProperty("UebDokumentZugRegelwegAbBstRefNavigation")]
    public virtual BasisBetriebsstelle RegelwegAbBstRefNavigation { get; set; } = null!;

    [ForeignKey("RegelwegZielBstRef")]
    [InverseProperty("UebDokumentZugRegelwegZielBstRefNavigation")]
    public virtual BasisBetriebsstelle RegelwegZielBstRefNavigation { get; set; } = null!;

    [ForeignKey("UebDokumentRef")]
    [InverseProperty("UebDokumentZug")]
    public virtual UebDokument UebDokumentRefNavigation { get; set; } = null!;

    [InverseProperty("UebDokumentZugRefNavigation")]
    public virtual ICollection<UebDokumentZugKnotenzeiten> UebDokumentZugKnotenzeiten { get; set; } = new List<UebDokumentZugKnotenzeiten>();

    [InverseProperty("UebDokumentZugRefNavigation")]
    public virtual ICollection<UebDokumentZugRegelung> UebDokumentZugRegelung { get; set; } = new List<UebDokumentZugRegelung>();
}
