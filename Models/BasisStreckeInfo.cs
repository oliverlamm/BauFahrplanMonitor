using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace BauFahrplanMonitor.Models;

[Table("basis_strecke_info", Schema = "ujbaudb")]
[Index("StreckenRef", "KmAnfangI", "KmEndeI", "Richtung", Name = "basis_strecke_info_uq_str_von_bis_dir", IsUnique = true)]
[Index("NetzRef", Name = "idx_basis_strecke_info_netz_ref")]
[Index("RegionRef", Name = "idx_basis_strecke_info_region_ref")]
public partial class BasisStreckeInfo
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("strecken_ref")]
    public long StreckenRef { get; set; }

    [Column("region_ref")]
    public long RegionRef { get; set; }

    [Column("km_anfang_i")]
    public long? KmAnfangI { get; set; }

    [Column("km_anfang_l", TypeName = "character varying")]
    public string? KmAnfangL { get; set; }

    [Column("km_ende_i")]
    public long? KmEndeI { get; set; }

    [Column("km_ende_l", TypeName = "character varying")]
    public string? KmEndeL { get; set; }

    [Column("netz_ref")]
    public long NetzRef { get; set; }

    [Column("art", TypeName = "character varying")]
    public string? Art { get; set; }

    [Column("gleisanzahl", TypeName = "character varying")]
    public string? Gleisanzahl { get; set; }

    [Column("richtung")]
    public long Richtung { get; set; }

    [Column("elektrifizierung", TypeName = "character varying")]
    public string? Elektrifizierung { get; set; }

    [Column("nutzung", TypeName = "character varying")]
    public string? Nutzung { get; set; }

    [Column("streckengeschwindigkeit", TypeName = "character varying")]
    public string? Streckengeschwindigkeit { get; set; }

    [Column("laenge", TypeName = "character varying")]
    public string? Laenge { get; set; }

    [Column("shape")]
    public Geometry? Shape { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [Column("ist_basis_datensatz")]
    public bool? IstBasisDatensatz { get; set; }

    [ForeignKey("NetzRef")]
    [InverseProperty("BasisStreckeInfo")]
    public virtual BasisNetz NetzRefNavigation { get; set; } = null!;

    [ForeignKey("RegionRef")]
    [InverseProperty("BasisStreckeInfo")]
    public virtual BasisRegion RegionRefNavigation { get; set; } = null!;

    [ForeignKey("StreckenRef")]
    [InverseProperty("BasisStreckeInfo")]
    public virtual BasisStrecke StreckenRefNavigation { get; set; } = null!;
}
