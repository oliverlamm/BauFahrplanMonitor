using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("basis_region", Schema = "ujbaudb")]
[Index("Id", "Kbez", Name = "basis_region_uq_id_kbez", IsUnique = true)]
public partial class BasisRegion
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("kbez", TypeName = "character varying")]
    public string? Kbez { get; set; }

    [Column("bezeichner", TypeName = "character varying")]
    public string? Bezeichner { get; set; }

    [Column("langname", TypeName = "character varying")]
    public string? Langname { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [Column("ist_basis_datensatz")]
    public bool? IstBasisDatensatz { get; set; }

    [InverseProperty("RegionRefNavigation")]
    public virtual ICollection<BasisBetriebsstelle> BasisBetriebsstelle { get; set; } = new List<BasisBetriebsstelle>();

    [InverseProperty("RegionRefNavigation")]
    public virtual ICollection<BasisStreckeInfo> BasisStreckeInfo { get; set; } = new List<BasisStreckeInfo>();

    [InverseProperty("RegionRefNavigation")]
    public virtual ICollection<BbpneoMassnahme> BbpneoMassnahme { get; set; } = new List<BbpneoMassnahme>();

    [InverseProperty("MasterRegionRefNavigation")]
    public virtual ICollection<FploDokument> FploDokumentMasterRegionRefNavigation { get; set; } = new List<FploDokument>();

    [InverseProperty("RegionRefNavigation")]
    public virtual ICollection<FploDokument> FploDokumentRegionRefNavigation { get; set; } = new List<FploDokument>();

    [InverseProperty("RegionRefNavigation")]
    public virtual ICollection<NfplZugVariante> NfplZugVariante { get; set; } = new List<NfplZugVariante>();

    [InverseProperty("RegionRefNavigation")]
    public virtual ICollection<UebDokument> UebDokument { get; set; } = new List<UebDokument>();

    [InverseProperty("RegionRefNavigation")]
    public virtual ICollection<ZvfDokument> ZvfDokument { get; set; } = new List<ZvfDokument>();
}
