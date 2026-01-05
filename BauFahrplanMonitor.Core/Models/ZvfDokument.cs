using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("zvf_dokument", Schema = "ujbaudb")]
[Index("RegionRef", Name = "idx_zvf_dokument_region_ref")]
[Index("SenderRef", Name = "idx_zvf_dokument_sender_ref")]
[Index("Dateiname", Name = "idx_zvfdok_dateiname")]
[Index("ImportTimestamp", "Dateiname", Name = "idx_zvfdok_import_timestamp")]
[Index("UjbauVorgangRef", Name = "idx_zvfdok_ujbau_vorgang_ref")]
[Index("UjbauVorgangRef", Name = "idx_zvfdok_ujbau_vorgang_ref2")]
[Index("ExportTimestamp", "Dateiname", Name = "zvf_dokument_unique", IsUnique = true)]
[Index("UjbauVorgangRef", "Dateiname", Name = "zvf_dokument_uq_vorgang_dateiname", IsUnique = true)]
public partial class ZvfDokument
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("export_timestamp", TypeName = "timestamp(6) without time zone")]
    public DateTime? ExportTimestamp { get; set; }

    [Column("import_timestamp", TypeName = "timestamp(6) without time zone")]
    public DateTime? ImportTimestamp { get; set; }

    [Column("sender_ref")]
    public long SenderRef { get; set; }

    [Column("ujbau_vorgang_ref")]
    public long UjbauVorgangRef { get; set; }

    [Column("version_major")]
    public long VersionMajor { get; set; }

    [Column("version_minor")]
    public long VersionMinor { get; set; }

    [Column("version_sub")]
    public long VersionSub { get; set; }

    [Column("endstueck")]
    public bool? Endstueck { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [Column("version")]
    public long Version { get; set; }

    [Column("region_ref")]
    public long RegionRef { get; set; }

    [Column("antwort_bis")]
    public DateOnly? AntwortBis { get; set; }

    [Column("baudatum_von")]
    public DateOnly? BaudatumVon { get; set; }

    [Column("baudatum_bis")]
    public DateOnly? BaudatumBis { get; set; }

    [Column("dateiname", TypeName = "character varying")]
    public string? Dateiname { get; set; }

    [Column("allgemein")]
    public string? Allgemein { get; set; }

    [ForeignKey("RegionRef")]
    [InverseProperty("ZvfDokument")]
    public virtual BasisRegion RegionRefNavigation { get; set; } = null!;

    [ForeignKey("SenderRef")]
    [InverseProperty("ZvfDokument")]
    public virtual UjbauSender SenderRefNavigation { get; set; } = null!;

    [ForeignKey("UjbauVorgangRef")]
    [InverseProperty("ZvfDokument")]
    public virtual UjbauVorgang UjbauVorgangRefNavigation { get; set; } = null!;

    [InverseProperty("ZvfDokumentRefNavigation")]
    public virtual ICollection<ZvfDokumentStreckenabschnitte> ZvfDokumentStreckenabschnitte { get; set; } = new List<ZvfDokumentStreckenabschnitte>();

    [InverseProperty("ZvfDokumentRefNavigation")]
    public virtual ICollection<ZvfDokumentZug> ZvfDokumentZug { get; set; } = new List<ZvfDokumentZug>();

    [InverseProperty("ZvfDokumentRefNavigation")]
    public virtual ICollection<ZvfDokumentZugEntfallen> ZvfDokumentZugEntfallen { get; set; } = new List<ZvfDokumentZugEntfallen>();
}
