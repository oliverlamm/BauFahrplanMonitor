using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("basis_netzbezirk", Schema = "ujbaudb")]
[Index("Id", "Name", "NetzRef", Name = "basis_netzbezirk_uq_id_name_netz", IsUnique = true)]
[Index("NetzRef", Name = "idx_basis_netzbezirk_netz_ref")]
public partial class BasisNetzbezirk
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("name", TypeName = "character varying")]
    public string? Name { get; set; }

    [Column("netz_ref")]
    public long NetzRef { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [Column("ist_basis_datensatz")]
    public bool? IstBasisDatensatz { get; set; }

    [InverseProperty("NetzbezirkRefNavigation")]
    public virtual ICollection<BasisBetriebsstelle> BasisBetriebsstelle { get; set; } = new List<BasisBetriebsstelle>();

    [ForeignKey("NetzRef")]
    [InverseProperty("BasisNetzbezirk")]
    public virtual BasisNetz NetzRefNavigation { get; set; } = null!;
}
