using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("ueb_dokument", Schema = "ujbaudb")]
[Index("UjbauVorgangRef", "Dateiname", Name = "ueb_dokument_uq_vorgang_dateiname", IsUnique = true)]
public partial class UebDokument
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("ujbau_vorgang_ref")]
    public long UjbauVorgangRef { get; set; }

    [Column("export_timestamp", TypeName = "timestamp without time zone")]
    public DateTime? ExportTimestamp { get; set; }

    [Column("import_timestamp", TypeName = "timestamp without time zone")]
    public DateTime? ImportTimestamp { get; set; }

    [Column("sender_ref")]
    public long SenderRef { get; set; }

    [Column("version_major")]
    public long VersionMajor { get; set; }

    [Column("version_minor")]
    public long VersionMinor { get; set; }

    [Column("version_sub")]
    public long VersionSub { get; set; }

    [Column("version")]
    public long Version { get; set; }

    [Column("region_ref")]
    public long? RegionRef { get; set; }

    [Column("gueltigkeit_von")]
    public DateOnly? GueltigkeitVon { get; set; }

    [Column("gueltigkeit_bis")]
    public DateOnly? GueltigkeitBis { get; set; }

    [Column("dateiname", TypeName = "character varying")]
    public string? Dateiname { get; set; }

    [Column("allgemein")]
    public string? Allgemein { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("RegionRef")]
    [InverseProperty("UebDokument")]
    public virtual BasisRegion? RegionRefNavigation { get; set; }

    [ForeignKey("SenderRef")]
    [InverseProperty("UebDokument")]
    public virtual UjbauSender SenderRefNavigation { get; set; } = null!;

    [InverseProperty("UebDokumentRefNavigation")]
    public virtual ICollection<UebDokumentStreckenabschnitte> UebDokumentStreckenabschnitte { get; set; } = new List<UebDokumentStreckenabschnitte>();

    [InverseProperty("UebDokumentRefNavigation")]
    public virtual ICollection<UebDokumentZug> UebDokumentZug { get; set; } = new List<UebDokumentZug>();

    [ForeignKey("UjbauVorgangRef")]
    [InverseProperty("UebDokument")]
    public virtual UjbauVorgang UjbauVorgangRefNavigation { get; set; } = null!;
}
