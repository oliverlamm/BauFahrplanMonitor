using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("bbpneo_massnahme_regelung_bve_iav", Schema = "ujbaudb")]
[Index("BbpneoMassnahmeRegelungBveRef", "VertragNr", Name = "bbpneo_massnahme_regelung_bve_iav_uq_ref_vertrag", IsUnique = true)]
[Index("Bst2strRef", Name = "idx_bbpneo_massnahme_regelung_bve_iav_bst2str_ref")]
public partial class BbpneoMassnahmeRegelungBveIav
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("bbpneo_massnahme_regelung_bve_ref")]
    public long BbpneoMassnahmeRegelungBveRef { get; set; }

    [Column("bst2str_ref")]
    public long Bst2strRef { get; set; }

    [Column("anschlussgrenze", TypeName = "character varying")]
    public string? Anschlussgrenze { get; set; }

    [Column("vertrag_nr", TypeName = "character varying")]
    public string? VertragNr { get; set; }

    [Column("vertrag_art", TypeName = "character varying")]
    public string? VertragArt { get; set; }

    [Column("vertrag_status", TypeName = "character varying")]
    public string? VertragStatus { get; set; }

    [Column("kunde", TypeName = "character varying")]
    public string? Kunde { get; set; }

    [Column("oberleitung")]
    public bool? Oberleitung { get; set; }

    [Column("oberleitung_aus")]
    public bool? OberleitungAus { get; set; }

    [Column("einschraenkung_bedienbarkeit_ia", TypeName = "character varying")]
    public string? EinschraenkungBedienbarkeitIa { get; set; }

    [Column("kommentar", TypeName = "character varying")]
    public string? Kommentar { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("BbpneoMassnahmeRegelungBveRef")]
    [InverseProperty("BbpneoMassnahmeRegelungBveIav")]
    public virtual BbpneoMassnahmeRegelungBve BbpneoMassnahmeRegelungBveRefNavigation { get; set; } = null!;

    [ForeignKey("Bst2strRef")]
    [InverseProperty("BbpneoMassnahmeRegelungBveIav")]
    public virtual BasisBetriebsstelle2strecke Bst2strRefNavigation { get; set; } = null!;
}
