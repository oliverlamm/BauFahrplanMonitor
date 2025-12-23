using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("nfpl_zug_zugteil_fk", Schema = "ujbaudb")]
[Index("NfplZugZugteilRef", Name = "idx_nfpl_zug_zugteil_fk_nfpl_zug_zugteil_ref")]
[Index("NfplZugZugteilRef", "Entryindextrassenid", Name = "uq_nfpl_fk_entryidx", IsUnique = true)]
public partial class NfplZugZugteilFk
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("nfpl_zug_zugteil_ref")]
    public long NfplZugZugteilRef { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [Column("entryindextrassenid")]
    public int? Entryindextrassenid { get; set; }

    [InverseProperty("NfplZugZugteilFkRefNavigation")]
    public virtual ICollection<NfplZugZugteilFkCon> NfplZugZugteilFkCon { get; set; } = new List<NfplZugZugteilFkCon>();

    [InverseProperty("NfplZugZugteilFkRefNavigation")]
    public virtual ICollection<NfplZugZugteilFkCt> NfplZugZugteilFkCt { get; set; } = new List<NfplZugZugteilFkCt>();

    [ForeignKey("NfplZugZugteilRef")]
    [InverseProperty("NfplZugZugteilFk")]
    public virtual NfplZugZugteil NfplZugZugteilRefNavigation { get; set; } = null!;
}
