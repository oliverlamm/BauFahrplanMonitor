using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("nfpl_zug_variante_verlauf", Schema = "ujbaudb")]
[Index("Seq", "NfplZugVarRef", Name = "nfpl_zug_var_verlauf_uq", IsUnique = true)]
[Index("NfplZugVarRef", "Seq", Name = "nfpl_zug_variante_verlauf_uq", IsUnique = true)]
public partial class NfplZugVarianteVerlauf
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("seq")]
    public long Seq { get; set; }

    [Column("bst_ref")]
    public long BstRef { get; set; }

    [Column("type", TypeName = "character varying")]
    public string Type { get; set; } = null!;

    [Column("published_arrival")]
    public TimeOnly? PublishedArrival { get; set; }

    [Column("published_departure")]
    public TimeOnly PublishedDeparture { get; set; }

    [Column("remarks", TypeName = "character varying")]
    public string? Remarks { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [Column("service_bitmask")]
    public string? ServiceBitmask { get; set; }

    [Column("service_startdate")]
    public DateOnly? ServiceStartdate { get; set; }

    [Column("service_enddate")]
    public DateOnly? ServiceEnddate { get; set; }

    [Column("service_description", TypeName = "character varying")]
    public string? ServiceDescription { get; set; }

    [Column("nfpl_zug_var_ref")]
    public long NfplZugVarRef { get; set; }

    [ForeignKey("BstRef")]
    [InverseProperty("NfplZugVarianteVerlauf")]
    public virtual BasisBetriebsstelle BstRefNavigation { get; set; } = null!;

    [ForeignKey("NfplZugVarRef")]
    [InverseProperty("NfplZugVarianteVerlauf")]
    public virtual NfplZugVariante NfplZugVarRefNavigation { get; set; } = null!;
}
