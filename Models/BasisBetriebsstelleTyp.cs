using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("basis_betriebsstelle_typ", Schema = "ujbaudb")]
[Index("Id", "Kbez", Name = "bst_typ_uq_id_kbez", IsUnique = true)]
public partial class BasisBetriebsstelleTyp
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("kbez", TypeName = "character varying")]
    public string? Kbez { get; set; }

    [Column("bezeichner", TypeName = "character varying")]
    public string? Bezeichner { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [Column("ist_basis_datensatz")]
    public bool? IstBasisDatensatz { get; set; }

    [InverseProperty("TypRefNavigation")]
    public virtual ICollection<BasisBetriebsstelle> BasisBetriebsstelle { get; set; } = new List<BasisBetriebsstelle>();
}
