using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("basis_betriebsstelle", Schema = "ujbaudb")]
[Index("NetzbezirkRef", Name = "idx_basis_betriebsstelle_netzbezirk_ref")]
[Index("RegionRef", Name = "idx_basis_betriebsstelle_region_ref")]
[Index("TypRef", Name = "idx_basis_betriebsstelle_typ_ref")]
[Index("Id", "Rl100", Name = "idx_bst_id")]
[Index("NetzbezirkRef", Name = "idx_bst_netzbezirk_ref")]
[Index("RegionRef", Name = "idx_bst_region_ref")]
[Index("TypRef", Name = "idx_bst_typ_ref")]
public partial class BasisBetriebsstelle
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("rl100", TypeName = "character varying")]
    public string? Rl100 { get; set; }

    [Column("name", TypeName = "character varying")]
    public string? Name { get; set; }

    [Column("typ_ref")]
    public long TypRef { get; set; }

    [Column("netzbezirk_ref")]
    public long NetzbezirkRef { get; set; }

    [Column("region_ref")]
    public long RegionRef { get; set; }

    [Column("zustand", TypeName = "character varying")]
    public string Zustand { get; set; } = null!;

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [Column("ist_basis_datensatz")]
    public bool? IstBasisDatensatz { get; set; }

    [InverseProperty("BstRefNavigation")]
    public virtual ICollection<BasisBetriebsstelle2strecke> BasisBetriebsstelle2strecke { get; set; } = new List<BasisBetriebsstelle2strecke>();

    [InverseProperty("BstRefNavigation")]
    public virtual ICollection<BbpneoMassnahmeRegelungBveAps> BbpneoMassnahmeRegelungBveAps { get; set; } = new List<BbpneoMassnahmeRegelungBveAps>();

    [InverseProperty("BstRefNavigation")]
    public virtual ICollection<FploDokumentZugFahrplan> FploDokumentZugFahrplan { get; set; } = new List<FploDokumentZugFahrplan>();

    [InverseProperty("AnkerBstRefNavigation")]
    public virtual ICollection<FploDokumentZugRegelung> FploDokumentZugRegelung { get; set; } = new List<FploDokumentZugRegelung>();

    [InverseProperty("RegelwegAbBstRefNavigation")]
    public virtual ICollection<FploDokumentZug> FploDokumentZugRegelwegAbBstRefNavigation { get; set; } = new List<FploDokumentZug>();

    [InverseProperty("RegelwegZielBstRefNavigation")]
    public virtual ICollection<FploDokumentZug> FploDokumentZugRegelwegZielBstRefNavigation { get; set; } = new List<FploDokumentZug>();

    [ForeignKey("NetzbezirkRef")]
    [InverseProperty("BasisBetriebsstelle")]
    public virtual BasisNetzbezirk NetzbezirkRefNavigation { get; set; } = null!;

    [InverseProperty("BstRefNavigation")]
    public virtual ICollection<NfplZugVarianteVerlauf> NfplZugVarianteVerlauf { get; set; } = new List<NfplZugVarianteVerlauf>();

    [ForeignKey("RegionRef")]
    [InverseProperty("BasisBetriebsstelle")]
    public virtual BasisRegion RegionRefNavigation { get; set; } = null!;

    [ForeignKey("TypRef")]
    [InverseProperty("BasisBetriebsstelle")]
    public virtual BasisBetriebsstelleTyp TypRefNavigation { get; set; } = null!;

    [InverseProperty("BstRefNavigation")]
    public virtual ICollection<UebDokumentZugKnotenzeiten> UebDokumentZugKnotenzeiten { get; set; } = new List<UebDokumentZugKnotenzeiten>();

    [InverseProperty("AnkerBstRefNavigation")]
    public virtual ICollection<UebDokumentZugRegelung> UebDokumentZugRegelung { get; set; } = new List<UebDokumentZugRegelung>();

    [InverseProperty("RegelwegAbBstRefNavigation")]
    public virtual ICollection<UebDokumentZug> UebDokumentZugRegelwegAbBstRefNavigation { get; set; } = new List<UebDokumentZug>();

    [InverseProperty("RegelwegZielBstRefNavigation")]
    public virtual ICollection<UebDokumentZug> UebDokumentZugRegelwegZielBstRefNavigation { get; set; } = new List<UebDokumentZug>();

    [InverseProperty("AbBstRefNavigation")]
    public virtual ICollection<ZvfDokumentZugAbweichung> ZvfDokumentZugAbweichung { get; set; } = new List<ZvfDokumentZugAbweichung>();

    [InverseProperty("RegelwegAbgangBstRefNavigation")]
    public virtual ICollection<ZvfDokumentZug> ZvfDokumentZugRegelwegAbgangBstRefNavigation { get; set; } = new List<ZvfDokumentZug>();

    [InverseProperty("RegelwegZielBstRefNavigation")]
    public virtual ICollection<ZvfDokumentZug> ZvfDokumentZugRegelwegZielBstRefNavigation { get; set; } = new List<ZvfDokumentZug>();
}
