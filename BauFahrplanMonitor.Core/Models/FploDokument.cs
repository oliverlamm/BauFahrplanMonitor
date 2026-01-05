using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("fplo_dokument", Schema = "ujbaudb")]
[Index("UjbauVorgangRef", "Dateiname", Name = "fplo_dokument_uq_vorgang_dateiname", IsUnique = true)]
[Index("MasterRegionRef", Name = "idx_fplo_dokument_master_region_ref")]
[Index("RegionRef", Name = "idx_fplo_dokument_region_ref")]
[Index("SenderRef", Name = "idx_fplo_dokument_sender_ref")]
public partial class FploDokument
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
    public long RegionRef { get; set; }

    [Column("gueltigkeit_von")]
    public DateOnly? GueltigkeitVon { get; set; }

    [Column("gueltigkeit_bis")]
    public DateOnly? GueltigkeitBis { get; set; }

    [Column("ist_nachtrag")]
    public bool? IstNachtrag { get; set; }

    [Column("ist_teillieferung")]
    public bool? IstTeillieferung { get; set; }

    [Column("master_region_ref")]
    public long MasterRegionRef { get; set; }

    [Column("dateiname", TypeName = "character varying")]
    public string? Dateiname { get; set; }

    [Column("allgemein")]
    public string? Allgemein { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [Column("ist_entwurf")]
    public bool IstEntwurf { get; set; }

    [InverseProperty("FploDokumentRefNavigation")]
    public virtual ICollection<FploDokumentStreckenabschnitte> FploDokumentStreckenabschnitte { get; set; } = new List<FploDokumentStreckenabschnitte>();

    [InverseProperty("FploDokumentRefNavigation")]
    public virtual ICollection<FploDokumentZug> FploDokumentZug { get; set; } = new List<FploDokumentZug>();

    [ForeignKey("MasterRegionRef")]
    [InverseProperty("FploDokumentMasterRegionRefNavigation")]
    public virtual BasisRegion MasterRegionRefNavigation { get; set; } = null!;

    [ForeignKey("RegionRef")]
    [InverseProperty("FploDokumentRegionRefNavigation")]
    public virtual BasisRegion RegionRefNavigation { get; set; } = null!;

    [ForeignKey("SenderRef")]
    [InverseProperty("FploDokument")]
    public virtual UjbauSender SenderRefNavigation { get; set; } = null!;

    [ForeignKey("UjbauVorgangRef")]
    [InverseProperty("FploDokument")]
    public virtual UjbauVorgang UjbauVorgangRefNavigation { get; set; } = null!;
}
