using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Core.Models;

[Table("basis_betriebsstellenbereich", Schema = "ujbaudb")]
[Index("BstRef", "BstChildRef", Name = "basis_betriebsstellenebreich_unique", IsUnique = true)]
public partial class BasisBetriebsstellenbereich
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("bst_ref")]
    public long BstRef { get; set; }

    [Column("bst_child_ref")]
    public long BstChildRef { get; set; }

    [ForeignKey("BstChildRef")]
    [InverseProperty("BasisBetriebsstellenbereichBstChildRefNavigation")]
    public virtual BasisBetriebsstelle BstChildRefNavigation { get; set; } = null!;

    [ForeignKey("BstRef")]
    [InverseProperty("BasisBetriebsstellenbereichBstRefNavigation")]
    public virtual BasisBetriebsstelle BstRefNavigation { get; set; } = null!;
}
