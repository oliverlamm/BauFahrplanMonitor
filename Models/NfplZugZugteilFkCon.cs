using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("nfpl_zug_zugteil_fk_con", Schema = "ujbaudb")]
[Index("ErsterServiceBstRef", Name = "idx_nfpl_zug_zugteil_fk_con_erster_service_bst_ref")]
[Index("NfplZugZugteilFkRef", Name = "idx_nfpl_zug_zugteil_fk_con_nfpl_zug_zugteil_fk_ref")]
[Index("NfplZugZugteilFkRef", "ErsterZugMainNumber", "ErsterZugSubNumber", "ZweiterZugMainNumber", "ZweiterZugSubNummer", Name = "uq_nfpl_fkcon_main_sub", IsUnique = true)]
public partial class NfplZugZugteilFkCon
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("nfpl_zug_zugteil_fk_ref")]
    public long NfplZugZugteilFkRef { get; set; }

    [Column("connection_mode", TypeName = "character varying")]
    public string? ConnectionMode { get; set; }

    [Column("erster_zug_main_number")]
    public int ErsterZugMainNumber { get; set; }

    [Column("erster_zug_sub_number")]
    public int ErsterZugSubNumber { get; set; }

    [Column("zweiter_zug_main_number")]
    public int? ZweiterZugMainNumber { get; set; }

    [Column("zweiter_zug_sub_nummer")]
    public int? ZweiterZugSubNummer { get; set; }

    [Column("erster_service_bst_ref")]
    public long? ErsterServiceBstRef { get; set; }

    [Column("zeit", TypeName = "character varying")]
    public string? Zeit { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("ErsterServiceBstRef")]
    [InverseProperty("NfplZugZugteilFkCon")]
    public virtual BasisBetriebsstelle? ErsterServiceBstRefNavigation { get; set; }

    [ForeignKey("NfplZugZugteilFkRef")]
    [InverseProperty("NfplZugZugteilFkCon")]
    public virtual NfplZugZugteilFk NfplZugZugteilFkRefNavigation { get; set; } = null!;
}
