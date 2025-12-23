using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("nfpl_zug_zugteil", Schema = "ujbaudb")]
[Index("NfplZugRef", "ZugNummer", "Fahrplan", Name = "uq_nfpl_zgt_zug_zref", IsUnique = true)]
public partial class NfplZugZugteil
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("nfpl_zug_ref")]
    public long NfplZugRef { get; set; }

    [Column("zug_nummer", TypeName = "character varying")]
    public string ZugNummer { get; set; } = null!;

    [Column("zugart", TypeName = "character varying")]
    public string? Zugart { get; set; }

    [Column("fahrplan")]
    public int Fahrplan { get; set; }

    [Column("bemerkung")]
    public string? Bemerkung { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [Column("zugid")]
    public int? Zugid { get; set; }

    [ForeignKey("NfplZugRef")]
    [InverseProperty("NfplZugZugteil")]
    public virtual NfplZug NfplZugRefNavigation { get; set; } = null!;

    [InverseProperty("NfplZugZugteilRefNavigation")]
    public virtual ICollection<NfplZugZugteilFahrtverlauf> NfplZugZugteilFahrtverlauf { get; set; } = new List<NfplZugZugteilFahrtverlauf>();

    [InverseProperty("NfplZugZugteilRefNavigation")]
    public virtual ICollection<NfplZugZugteilFk> NfplZugZugteilFk { get; set; } = new List<NfplZugZugteilFk>();
}
