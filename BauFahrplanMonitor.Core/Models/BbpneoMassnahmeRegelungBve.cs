using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("bbpneo_massnahme_regelung_bve", Schema = "ujbaudb")]
[Index("BbpneoMasRegRef", "BveId", Name = "bbpneo_massnahme_regelung_bve_uq", IsUnique = true)]
[Index("Bst2strBisRef", Name = "idx_bbpneo_massnahme_regelung_bve_bst2str_bis_ref")]
[Index("Bst2strVonRef", Name = "idx_bbpneo_massnahme_regelung_bve_bst2str_von_ref")]
public partial class BbpneoMassnahmeRegelungBve
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("bve_id", TypeName = "character varying")]
    public string BveId { get; set; } = null!;

    [Column("aktiv")]
    public bool? Aktiv { get; set; }

    [Column("art", TypeName = "character varying")]
    public string? Art { get; set; }

    [Column("bst2str_von_ref")]
    public long Bst2strVonRef { get; set; }

    [Column("bst2str_bis_ref")]
    public long Bst2strBisRef { get; set; }

    [Column("ort_mikroskopisch", TypeName = "character varying")]
    public string? OrtMikroskopisch { get; set; }

    [Column("bemerkung", TypeName = "character varying")]
    public string? Bemerkung { get; set; }

    [Column("iav_betroffenheit")]
    public bool IavBetroffenheit { get; set; }

    [Column("iav_beschreibung", TypeName = "character varying")]
    public string? IavBeschreibung { get; set; }

    [Column("aps_betroffenheit")]
    public bool ApsBetroffenheit { get; set; }

    [Column("aps_beschreibung", TypeName = "character varying")]
    public string? ApsBeschreibung { get; set; }

    [Column("aps_frei_von_fahrzeugen")]
    public bool? ApsFreiVonFahrzeugen { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [Column("bbpneo_mas_reg_ref")]
    public long BbpneoMasRegRef { get; set; }

    [Column("gueltigkeit", TypeName = "character varying")]
    public string? Gueltigkeit { get; set; }

    [Column("gueltigkeit_von", TypeName = "timestamp without time zone")]
    public DateTime? GueltigkeitVon { get; set; }

    [Column("gueltigkeit_bis", TypeName = "timestamp without time zone")]
    public DateTime? GueltigkeitBis { get; set; }

    [Column("gueltigkeit_effektive_verkehrstage", TypeName = "character varying")]
    public string? GueltigkeitEffektiveVerkehrstage { get; set; }

    [ForeignKey("BbpneoMasRegRef")]
    [InverseProperty("BbpneoMassnahmeRegelungBve")]
    public virtual BbpneoMassnahmeRegelung BbpneoMasRegRefNavigation { get; set; } = null!;

    [InverseProperty("BbpneoMassnahmeRegelungBveRefNavigation")]
    public virtual ICollection<BbpneoMassnahmeRegelungBveAps> BbpneoMassnahmeRegelungBveAps { get; set; } = new List<BbpneoMassnahmeRegelungBveAps>();

    [InverseProperty("BbpneoMassnahmeRegelungBveRefNavigation")]
    public virtual ICollection<BbpneoMassnahmeRegelungBveIav> BbpneoMassnahmeRegelungBveIav { get; set; } = new List<BbpneoMassnahmeRegelungBveIav>();

    [ForeignKey("Bst2strBisRef")]
    [InverseProperty("BbpneoMassnahmeRegelungBveBst2strBisRefNavigation")]
    public virtual BasisBetriebsstelle2strecke Bst2strBisRefNavigation { get; set; } = null!;

    [ForeignKey("Bst2strVonRef")]
    [InverseProperty("BbpneoMassnahmeRegelungBveBst2strVonRefNavigation")]
    public virtual BasisBetriebsstelle2strecke Bst2strVonRefNavigation { get; set; } = null!;
}
