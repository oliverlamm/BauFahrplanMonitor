using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("ueb_dokument_streckenabschnitte", Schema = "ujbaudb")]
[Index("UebDokumentRef", "Streckenabschnitt", Name = "uq_ueb_strabs_dok_abschnitt", IsUnique = true)]
public partial class UebDokumentStreckenabschnitte
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("ueb_dokument_ref")]
    public long? UebDokumentRef { get; set; }

    [Column("streckenabschnitt", TypeName = "jsonb")]
    public string? Streckenabschnitt { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("UebDokumentRef")]
    [InverseProperty("UebDokumentStreckenabschnitte")]
    public virtual UebDokument? UebDokumentRefNavigation { get; set; }
}
