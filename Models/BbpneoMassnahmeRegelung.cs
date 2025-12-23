using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("bbpneo_massnahme_regelung", Schema = "ujbaudb")]
[Index("RegId", Name = "bbpneo_massnahme_regelung_uq_reg_id", IsUnique = true)]
[Index("BbpneoMasRef", Name = "idx_bbpneo_massnahme_regelung_bbpneo_mas_ref")]
[Index("Bst2strBisRef", Name = "idx_bbpneo_massnahme_regelung_bst2str_bis_ref")]
[Index("Bst2strVonRef", Name = "idx_bbpneo_massnahme_regelung_bst2str_von_ref")]
public partial class BbpneoMassnahmeRegelung
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("bbpneo_mas_ref")]
    public long BbpneoMasRef { get; set; }

    [Column("reg_id", TypeName = "character varying")]
    public string? RegId { get; set; }

    [Column("aktiv")]
    public bool? Aktiv { get; set; }

    [Column("bplart", TypeName = "character varying")]
    public string Bplart { get; set; } = null!;

    [Column("beginn", TypeName = "timestamp without time zone")]
    public DateTime Beginn { get; set; }

    [Column("ende", TypeName = "timestamp without time zone")]
    public DateTime Ende { get; set; }

    [Column("bst2str_von_ref")]
    public long Bst2strVonRef { get; set; }

    [Column("bst2str_bis_ref")]
    public long Bst2strBisRef { get; set; }

    [Column("zeitraum", TypeName = "character varying")]
    public string Zeitraum { get; set; } = null!;

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [Column("richtung")]
    public short? Richtung { get; set; }

    [Column("regelung_kurz", TypeName = "character varying")]
    public string? RegelungKurz { get; set; }

    [Column("regelung_lang", TypeName = "character varying")]
    public string? RegelungLang { get; set; }

    [Column("durchgehend")]
    public bool? Durchgehend { get; set; }

    [Column("schichtweise")]
    public bool? Schichtweise { get; set; }

    [ForeignKey("BbpneoMasRef")]
    [InverseProperty("BbpneoMassnahmeRegelung")]
    public virtual BbpneoMassnahme BbpneoMasRefNavigation { get; set; } = null!;

    [InverseProperty("BbpneoMasRegRefNavigation")]
    public virtual ICollection<BbpneoMassnahmeRegelungBve> BbpneoMassnahmeRegelungBve { get; set; } = new List<BbpneoMassnahmeRegelungBve>();

    [ForeignKey("Bst2strBisRef")]
    [InverseProperty("BbpneoMassnahmeRegelungBst2strBisRefNavigation")]
    public virtual BasisBetriebsstelle2strecke Bst2strBisRefNavigation { get; set; } = null!;

    [ForeignKey("Bst2strVonRef")]
    [InverseProperty("BbpneoMassnahmeRegelungBst2strVonRefNavigation")]
    public virtual BasisBetriebsstelle2strecke Bst2strVonRefNavigation { get; set; } = null!;
}
