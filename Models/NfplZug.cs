using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("nfpl_zug", Schema = "ujbaudb")]
[Index("ZugNr", Name = "nfpl_zug_uq_zugnr", IsUnique = true)]
[Index("ZugNr", Name = "uq_nfpl_zug_zugnr", IsUnique = true)]
public partial class NfplZug
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("zug_nr")]
    public long ZugNr { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime UpdatedAt { get; set; }

    [InverseProperty("NfplZugRefNavigation")]
    public virtual ICollection<NfplZugZugteil> NfplZugZugteil { get; set; } = new List<NfplZugZugteil>();
}
