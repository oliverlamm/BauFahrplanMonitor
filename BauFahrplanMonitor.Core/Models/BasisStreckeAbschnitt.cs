using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Core.Models;

[Table("basis_strecke_abschnitt", Schema = "ujbaudb")]
[Index("StreckeRef", "VonBstRef", "BisBstRef", Name = "basis_strecke_abschnitt_unique", IsUnique = true)]
public partial class BasisStreckeAbschnitt
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("strecke_ref")]
    public long StreckeRef { get; set; }

    [Column("von_bst_ref")]
    public long VonBstRef { get; set; }

    [Column("bis_bst_ref")]
    public long BisBstRef { get; set; }

    [Column("von_km_i")]
    public decimal? VonKmI { get; set; }

    [Column("bis_km_i")]
    public decimal? BisKmI { get; set; }

    [ForeignKey("BisBstRef")]
    [InverseProperty("BasisStreckeAbschnittBisBstRefNavigation")]
    public virtual BasisBetriebsstelle BisBstRefNavigation { get; set; } = null!;

    [ForeignKey("StreckeRef")]
    [InverseProperty("BasisStreckeAbschnitt")]
    public virtual BasisStrecke StreckeRefNavigation { get; set; } = null!;

    [ForeignKey("VonBstRef")]
    [InverseProperty("BasisStreckeAbschnittVonBstRefNavigation")]
    public virtual BasisBetriebsstelle VonBstRefNavigation { get; set; } = null!;
}
