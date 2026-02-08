using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Core.Models;

[Table("zvf_dokument_zug", Schema = "ujbaudb")]
[Index("KundeRef", Name = "idx_zvf_dokument_zug_kunde_ref")]
[Index("RegelwegAbgangBstRef", Name = "idx_zvf_dokument_zug_regelweg_abgang_bst_ref")]
[Index("RegelwegZielBstRef", Name = "idx_zvf_dokument_zug_regelweg_ziel_bst_ref")]
[Index("ZvfDokumentRef", Name = "idx_zvf_dokument_zug_zvf_dokument_ref")]
[Index("ZvfDokumentRef", "Zugnr", "Verkehrstag", Name = "idx_zvf_dokzug_dok_zug_tag")]
[Index("Verkehrstag", "Zugnr", Name = "idx_zvfzug_verkehrstag")]
[Index("Zugnr", Name = "idx_zvfzug_zugnr")]
[Index("ZvfDokumentRef", Name = "idx_zvfzug_zvf_dokument_ref")]
[Index("Verkehrstag", "Zugnr", "ZvfDokumentRef", Name = "zvf_dokument_zug_uq_tag_zug_dok", IsUnique = true)]
public partial class ZvfDokumentZug
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("aenderung", TypeName = "character varying")]
    public string? Aenderung { get; set; }

    [Column("verkehrstag")]
    public DateOnly Verkehrstag { get; set; }

    [Column("zugnr")]
    public long Zugnr { get; set; }

    [Column("zugbez", TypeName = "character varying")]
    public string? Zugbez { get; set; }

    [Column("kunde_ref")]
    public long KundeRef { get; set; }

    [Column("regelweg_linie", TypeName = "character varying")]
    public string? RegelwegLinie { get; set; }

    [Column("regelweg_abgang_bst_ref")]
    public long? RegelwegAbgangBstRef { get; set; }

    [Column("regelweg_ziel_bst_ref")]
    public long? RegelwegZielBstRef { get; set; }

    [Column("klv", TypeName = "character varying")]
    public string? Klv { get; set; }

    [Column("skl", TypeName = "character varying")]
    public string? Skl { get; set; }

    [Column("bza", TypeName = "character varying")]
    public string? Bza { get; set; }

    [Column("update_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdateAt { get; set; }

    [Column("zvf_dokument_ref")]
    public long ZvfDokumentRef { get; set; }

    [Column("bemerkung")]
    public string? Bemerkung { get; set; }

    [Column("bedarf")]
    public bool? Bedarf { get; set; }

    [Column("sonderzug")]
    public bool? Sonderzug { get; set; }

    [ForeignKey("KundeRef")]
    [InverseProperty("ZvfDokumentZug")]
    public virtual BasisKunde KundeRefNavigation { get; set; } = null!;

    [ForeignKey("RegelwegAbgangBstRef")]
    [InverseProperty("ZvfDokumentZugRegelwegAbgangBstRefNavigation")]
    public virtual BasisBetriebsstelle? RegelwegAbgangBstRefNavigation { get; set; }

    [ForeignKey("RegelwegZielBstRef")]
    [InverseProperty("ZvfDokumentZugRegelwegZielBstRefNavigation")]
    public virtual BasisBetriebsstelle? RegelwegZielBstRefNavigation { get; set; }

    [ForeignKey("ZvfDokumentRef")]
    [InverseProperty("ZvfDokumentZug")]
    public virtual ZvfDokument ZvfDokumentRefNavigation { get; set; } = null!;

    [InverseProperty("ZvfDokumentZugRefNavigation")]
    public virtual ICollection<ZvfDokumentZugAbweichung> ZvfDokumentZugAbweichung { get; set; } = new List<ZvfDokumentZugAbweichung>();
}
