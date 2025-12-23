using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace BauFahrplanMonitor.Models;

[Table("basis_betriebsstelle2strecke", Schema = "ujbaudb")]
[Index("BstRef", "StreckeRef", Name = "bst2str_uq_bst_str", IsUnique = true)]
[Index("StreckeRef", Name = "idx_basis_betriebsstelle2strecke_strecke_ref")]
public partial class BasisBetriebsstelle2strecke
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("bst_ref")]
    public long BstRef { get; set; }

    [Column("strecke_ref")]
    public long StreckeRef { get; set; }

    [Column("km_i")]
    public long KmI { get; set; }

    [Column("km_l", TypeName = "character varying")]
    public string? KmL { get; set; }

    [Column("shape")]
    public Geometry? Shape { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime UpdatedAt { get; set; }

    [Column("ist_basis_datensatz")]
    public bool? IstBasisDatensatz { get; set; }

    [InverseProperty("MasBisBst2strRefNavigation")]
    public virtual ICollection<BbpneoMassnahme> BbpneoMassnahmeMasBisBst2strRefNavigation { get; set; } = new List<BbpneoMassnahme>();

    [InverseProperty("MasVonBst2strRefNavigation")]
    public virtual ICollection<BbpneoMassnahme> BbpneoMassnahmeMasVonBst2strRefNavigation { get; set; } = new List<BbpneoMassnahme>();

    [InverseProperty("Bst2strBisRefNavigation")]
    public virtual ICollection<BbpneoMassnahmeRegelung> BbpneoMassnahmeRegelungBst2strBisRefNavigation { get; set; } = new List<BbpneoMassnahmeRegelung>();

    [InverseProperty("Bst2strVonRefNavigation")]
    public virtual ICollection<BbpneoMassnahmeRegelung> BbpneoMassnahmeRegelungBst2strVonRefNavigation { get; set; } = new List<BbpneoMassnahmeRegelung>();

    [InverseProperty("Bst2strBisRefNavigation")]
    public virtual ICollection<BbpneoMassnahmeRegelungBve> BbpneoMassnahmeRegelungBveBst2strBisRefNavigation { get; set; } = new List<BbpneoMassnahmeRegelungBve>();

    [InverseProperty("Bst2strVonRefNavigation")]
    public virtual ICollection<BbpneoMassnahmeRegelungBve> BbpneoMassnahmeRegelungBveBst2strVonRefNavigation { get; set; } = new List<BbpneoMassnahmeRegelungBve>();

    [InverseProperty("Bst2strRefNavigation")]
    public virtual ICollection<BbpneoMassnahmeRegelungBveIav> BbpneoMassnahmeRegelungBveIav { get; set; } = new List<BbpneoMassnahmeRegelungBveIav>();

    [ForeignKey("BstRef")]
    [InverseProperty("BasisBetriebsstelle2strecke")]
    public virtual BasisBetriebsstelle BstRefNavigation { get; set; } = null!;

    [ForeignKey("StreckeRef")]
    [InverseProperty("BasisBetriebsstelle2strecke")]
    public virtual BasisStrecke StreckeRefNavigation { get; set; } = null!;
}
