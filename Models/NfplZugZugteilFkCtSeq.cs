using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("nfpl_zug_zugteil_fk_ct_seq", Schema = "ujbaudb")]
[Index("BstRef", Name = "idx_nfpl_zug_zugteil_fk_ct_seq_bst_ref")]
[Index("NfplZugZugteilFkCtRef", "BstRef", Name = "uq_nfpl_ctseq_fkct_bst", IsUnique = true)]
public partial class NfplZugZugteilFkCtSeq
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("nfpl_zug_zugteil_fk_ct_ref")]
    public long NfplZugZugteilFkCtRef { get; set; }

    [Column("bst_ref")]
    public long BstRef { get; set; }

    [Column("ankunft_zeit")]
    public TimeOnly? AnkunftZeit { get; set; }

    [Column("veroeffentlichte_ankunft_zeit")]
    public TimeOnly? VeroeffentlichteAnkunftZeit { get; set; }

    [Column("abfahrt_zeit")]
    public TimeOnly AbfahrtZeit { get; set; }

    [Column("veroeffentlichte_abfahrt_zeit")]
    public TimeOnly? VeroeffentlichteAbfahrtZeit { get; set; }

    [Column("haltart", TypeName = "character varying")]
    public string? Haltart { get; set; }

    [Column("min_haltezeit", TypeName = "character varying")]
    public string? MinHaltezeit { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("BstRef")]
    [InverseProperty("NfplZugZugteilFkCtSeq")]
    public virtual BasisBetriebsstelle BstRefNavigation { get; set; } = null!;

    [ForeignKey("NfplZugZugteilFkCtRef")]
    [InverseProperty("NfplZugZugteilFkCtSeq")]
    public virtual NfplZugZugteilFkCt NfplZugZugteilFkCtRefNavigation { get; set; } = null!;
}
