using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("basis_netz", Schema = "ujbaudb")]
[Index("Id", "Name", Name = "basis_netz_uq_id_name", IsUnique = true)]
public partial class BasisNetz
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("name", TypeName = "character varying")]
    public string? Name { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [Column("ist_basis_datensatz")]
    public bool? IstBasisDatensatz { get; set; }

    [InverseProperty("NetzRefNavigation")]
    public virtual ICollection<BasisNetzbezirk> BasisNetzbezirk { get; set; } = new List<BasisNetzbezirk>();

    [InverseProperty("NetzRefNavigation")]
    public virtual ICollection<BasisStreckeInfo> BasisStreckeInfo { get; set; } = new List<BasisStreckeInfo>();
}
