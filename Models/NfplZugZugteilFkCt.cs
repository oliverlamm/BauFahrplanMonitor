using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("nfpl_zug_zugteil_fk_ct", Schema = "ujbaudb")]
[Index("NfplZugZugteilFkRef", Name = "idx_nfpl_zug_zugteil_fk_ct_nfpl_zug_zugteil_fk_ref")]
[Index("NfplZugZugteilFkRef", "ZugnrMain", "ZugnrSub", Name = "uq_nfpl_fkct_main_sub", IsUnique = true)]
public partial class NfplZugZugteilFkCt
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("nfpl_zug_zugteil_fk_ref")]
    public long NfplZugZugteilFkRef { get; set; }

    [Column("zugnr_main")]
    public int ZugnrMain { get; set; }

    [Column("zugnr_sub")]
    public int ZugnrSub { get; set; }

    [Column("service_beschreibung")]
    public string? ServiceBeschreibung { get; set; }

    [Column("service_start_datum")]
    public DateOnly ServiceStartDatum { get; set; }

    [Column("service_end_datum")]
    public DateOnly? ServiceEndDatum { get; set; }

    [Column("service_bitmask")]
    public string ServiceBitmask { get; set; } = null!;

    [Column("service_operating_day", TypeName = "character varying")]
    public string? ServiceOperatingDay { get; set; }

    [Column("service_tagestyp", TypeName = "character varying")]
    public string? ServiceTagestyp { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("NfplZugZugteilFkCtRefNavigation")]
    public virtual ICollection<NfplZugZugteilFkCtSeq> NfplZugZugteilFkCtSeq { get; set; } = new List<NfplZugZugteilFkCtSeq>();

    [ForeignKey("NfplZugZugteilFkRef")]
    [InverseProperty("NfplZugZugteilFkCt")]
    public virtual NfplZugZugteilFk NfplZugZugteilFkRefNavigation { get; set; } = null!;
}
