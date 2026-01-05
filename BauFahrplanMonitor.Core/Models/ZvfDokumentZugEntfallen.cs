using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("zvf_dokument_zug_entfallen", Schema = "ujbaudb")]
[Index("ZvfDokumentRef", "Art", "Verkehrstag", "Zugnr", Name = "zvf_zug_entf_uq_dok_art_tag_zug", IsUnique = true)]
public partial class ZvfDokumentZugEntfallen
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("zvf_dokument_ref")]
    public long ZvfDokumentRef { get; set; }

    [Column("art", TypeName = "character varying")]
    public string Art { get; set; } = null!;

    [Column("verkehrstag")]
    public DateOnly Verkehrstag { get; set; }

    [Column("zugnr")]
    public int Zugnr { get; set; }

    [Column("zugbez", TypeName = "character varying")]
    public string? Zugbez { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("ZvfDokumentRef")]
    [InverseProperty("ZvfDokumentZugEntfallen")]
    public virtual ZvfDokument ZvfDokumentRefNavigation { get; set; } = null!;
}
