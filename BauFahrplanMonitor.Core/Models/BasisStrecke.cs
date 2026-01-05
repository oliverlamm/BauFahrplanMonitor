using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("basis_strecke", Schema = "ujbaudb")]
[Index("VzgNr", Name = "basis_strecke_uq_vzg_nr", IsUnique = true)]
public partial class BasisStrecke
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("vzg_nr")]
    public long VzgNr { get; set; }

    [Column("bezeichner", TypeName = "character varying")]
    public string? Bezeichner { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [Column("ist_basis_datensatz")]
    public bool? IstBasisDatensatz { get; set; }

    [InverseProperty("StreckeRefNavigation")]
    public virtual ICollection<BasisBetriebsstelle2strecke> BasisBetriebsstelle2strecke { get; set; } = new List<BasisBetriebsstelle2strecke>();

    [InverseProperty("StreckenRefNavigation")]
    public virtual ICollection<BasisStreckeInfo> BasisStreckeInfo { get; set; } = new List<BasisStreckeInfo>();
}
