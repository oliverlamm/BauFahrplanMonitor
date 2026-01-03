using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("nfpl_zug_variante", Schema = "ujbaudb")]
[Index("NfplZugRef", "TrainId", "RegionRef", Name = "nfpl_zug_var_uq", IsUnique = true)]
[Index("NfplZugRef", "TrainId", "TrainNumber", Name = "nfpl_zug_variante_nfpl_zug_ref_idx")]
public partial class NfplZugVariante
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("nfpl_zug_ref")]
    public long NfplZugRef { get; set; }

    [Column("train_id")]
    public long? TrainId { get; set; }

    [Column("train_number", TypeName = "character varying")]
    public string? TrainNumber { get; set; }

    [Column("kind", TypeName = "character varying")]
    public string? Kind { get; set; }

    [Column("remarks", TypeName = "character varying")]
    public string? Remarks { get; set; }

    [Column("train_status", TypeName = "character varying")]
    public string? TrainStatus { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [Column("region_ref")]
    public long RegionRef { get; set; }

    [ForeignKey("NfplZugRef")]
    [InverseProperty("NfplZugVariante")]
    public virtual NfplZug NfplZugRefNavigation { get; set; } = null!;

    [InverseProperty("NfplZugVarRefNavigation")]
    public virtual ICollection<NfplZugVarianteVerlauf> NfplZugVarianteVerlauf { get; set; } = new List<NfplZugVarianteVerlauf>();

    [ForeignKey("RegionRef")]
    [InverseProperty("NfplZugVariante")]
    public virtual BasisRegion RegionRefNavigation { get; set; } = null!;
}
