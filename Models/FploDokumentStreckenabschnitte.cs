using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("fplo_dokument_streckenabschnitte", Schema = "ujbaudb")]
[Index("FploDokumentRef", "Streckenabschnitt", Name = "uq_fplo_strabs_dok_abschnitt", IsUnique = true)]
public partial class FploDokumentStreckenabschnitte
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("fplo_dokument_ref")]
    public long? FploDokumentRef { get; set; }

    [Column("streckenabschnitt", TypeName = "jsonb")]
    public string? Streckenabschnitt { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("FploDokumentRef")]
    [InverseProperty("FploDokumentStreckenabschnitte")]
    public virtual FploDokument? FploDokumentRefNavigation { get; set; }
}
