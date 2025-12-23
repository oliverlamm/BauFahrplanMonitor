using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("bbpneo_massnahme", Schema = "ujbaudb")]
[Index("MasId", Name = "bbpneo_massnahme_uq_mas_id", IsUnique = true)]
[Index("MasBisBst2strRef", Name = "idx_bbpneo_massnahme_mas_bis_bst2str_ref")]
[Index("MasVonBst2strRef", Name = "idx_bbpneo_massnahme_mas_von_bst2str_ref")]
[Index("RegionRef", Name = "idx_bbpneo_massnahme_region_ref")]
public partial class BbpneoMassnahme
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("mas_id", TypeName = "character varying")]
    public string? MasId { get; set; }

    [Column("aktiv")]
    public bool? Aktiv { get; set; }

    [Column("region_ref")]
    public long RegionRef { get; set; }

    [Column("arbeiten", TypeName = "character varying")]
    public string Arbeiten { get; set; } = null!;

    [Column("art_der_arbeit", TypeName = "character varying")]
    public string? ArtDerArbeit { get; set; }

    [Column("mas_von_bst2str_ref")]
    public long MasVonBst2strRef { get; set; }

    [Column("mas_bis_bst2str_ref")]
    public long MasBisBst2strRef { get; set; }

    [Column("mas_von_km_l", TypeName = "character varying")]
    public string? MasVonKmL { get; set; }

    [Column("mas_bis_km_l", TypeName = "character varying")]
    public string? MasBisKmL { get; set; }

    [Column("mas_beginn", TypeName = "timestamp without time zone")]
    public DateTime MasBeginn { get; set; }

    [Column("mas_ende", TypeName = "timestamp without time zone")]
    public DateTime MasEnde { get; set; }

    [Column("genehmigung", TypeName = "character varying")]
    public string? Genehmigung { get; set; }

    [Column("anforderung_bbzr")]
    public DateOnly? AnforderungBbzr { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("BbpneoMasRefNavigation")]
    public virtual ICollection<BbpneoMassnahmeRegelung> BbpneoMassnahmeRegelung { get; set; } = new List<BbpneoMassnahmeRegelung>();

    [ForeignKey("MasBisBst2strRef")]
    [InverseProperty("BbpneoMassnahmeMasBisBst2strRefNavigation")]
    public virtual BasisBetriebsstelle2strecke MasBisBst2strRefNavigation { get; set; } = null!;

    [ForeignKey("MasVonBst2strRef")]
    [InverseProperty("BbpneoMassnahmeMasVonBst2strRefNavigation")]
    public virtual BasisBetriebsstelle2strecke MasVonBst2strRefNavigation { get; set; } = null!;

    [ForeignKey("RegionRef")]
    [InverseProperty("BbpneoMassnahme")]
    public virtual BasisRegion RegionRefNavigation { get; set; } = null!;
}
