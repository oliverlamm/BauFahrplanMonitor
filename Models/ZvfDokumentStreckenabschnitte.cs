using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("zvf_dokument_streckenabschnitte", Schema = "ujbaudb")]
[Index("ZvfDokumentRef", Name = "uq_zvf_strabs_dok", IsUnique = true)]
public partial class ZvfDokumentStreckenabschnitte
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("zvf_dokument_ref")]
    public long ZvfDokumentRef { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [Column("streckenabschnitt", TypeName = "jsonb")]
    public string Streckenabschnitt { get; set; } = null!;

    [ForeignKey("ZvfDokumentRef")]
    [InverseProperty("ZvfDokumentStreckenabschnitte")]
    public virtual ZvfDokument ZvfDokumentRefNavigation { get; set; } = null!;
}
