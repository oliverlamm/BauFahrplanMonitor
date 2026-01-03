using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("nfpl_zug", Schema = "ujbaudb")]
public partial class NfplZug
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("zug_nr")]
    public long ZugNr { get; set; }

    [Column("fahrplan_jahr")]
    public int FahrplanJahr { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("NfplZugRefNavigation")]
    public virtual ICollection<NfplZugVariante> NfplZugVariante { get; set; } = new List<NfplZugVariante>();
}
