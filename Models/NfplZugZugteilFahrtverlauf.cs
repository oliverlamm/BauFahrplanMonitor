using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("nfpl_zug_zugteil_fahrtverlauf", Schema = "ujbaudb")]
[Index("BstRef", Name = "idx_nfpl_zug_zugteil_fahrtverlauf_bst_ref")]
[Index("NfplZugZugteilRef", "BstRef", "Lfdnr", Name = "uq_nfpl_fv_zgt_bst_lfdnr", IsUnique = true)]
public partial class NfplZugZugteilFahrtverlauf
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("nfpl_zug_zugteil_ref")]
    public long NfplZugZugteilRef { get; set; }

    [Column("bst_ref")]
    public long BstRef { get; set; }

    [Column("typ", TypeName = "character varying")]
    public string? Typ { get; set; }

    [Column("published_arrival")]
    public TimeOnly? PublishedArrival { get; set; }

    [Column("published_departure")]
    public TimeOnly PublishedDeparture { get; set; }

    [Column("gleis", TypeName = "character varying")]
    public string? Gleis { get; set; }

    [Column("service_bitmask")]
    public string ServiceBitmask { get; set; } = null!;

    [Column("service_start_datum")]
    public DateOnly ServiceStartDatum { get; set; }

    [Column("service_end_datum")]
    public DateOnly? ServiceEndDatum { get; set; }

    [Column("service_beschreibung", TypeName = "character varying")]
    public string? ServiceBeschreibung { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [Column("lfdnr")]
    public int Lfdnr { get; set; }

    [ForeignKey("BstRef")]
    [InverseProperty("NfplZugZugteilFahrtverlauf")]
    public virtual BasisBetriebsstelle BstRefNavigation { get; set; } = null!;

    [ForeignKey("NfplZugZugteilRef")]
    [InverseProperty("NfplZugZugteilFahrtverlauf")]
    public virtual NfplZugZugteil NfplZugZugteilRefNavigation { get; set; } = null!;
}
