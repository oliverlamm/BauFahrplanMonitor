using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("bbpneo_massnahme_regelung_bve_aps", Schema = "ujbaudb")]
[Index("BbpneoMassnahmeRegelungBveRef", "Uuid", Name = "bbpneo_massnahme_regelung_bve_aps_uq_ref_uuid", IsUnique = true)]
[Index("BstRef", Name = "idx_bbpneo_massnahme_regelung_bve_aps_bst_ref")]
public partial class BbpneoMassnahmeRegelungBveAps
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("bbpneo_massnahme_regelung_bve_ref")]
    public long BbpneoMassnahmeRegelungBveRef { get; set; }

    [Column("ab_fahrplanjahr")]
    public int? AbFahrplanjahr { get; set; }

    [Column("uuid", TypeName = "character varying")]
    public string Uuid { get; set; } = null!;

    [Column("bst_ref")]
    public long? BstRef { get; set; }

    [Column("gleis", TypeName = "character varying")]
    public string? Gleis { get; set; }

    [Column("primaere_kategorie", TypeName = "character varying")]
    public string? PrimaereKategorie { get; set; }

    [Column("sekundaere_kategorie", TypeName = "character varying")]
    public string? SekundaereKategorie { get; set; }

    [Column("oberleitung")]
    public bool? Oberleitung { get; set; }

    [Column("oberleitung_aus")]
    public bool? OberleitungAus { get; set; }

    [Column("technischer_platz", TypeName = "character varying")]
    public string? TechnischerPlatz { get; set; }

    [Column("art_der_anbindung", TypeName = "character varying")]
    public string? ArtDerAnbindung { get; set; }

    [Column("einschraenkung_befahrbarkeit_se", TypeName = "character varying")]
    public string? EinschraenkungBefahrbarkeitSe { get; set; }

    [Column("kommentar")]
    public string? Kommentar { get; set; }

    [Column("moegliche_za", TypeName = "jsonb")]
    public string? MoeglicheZa { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("BbpneoMassnahmeRegelungBveRef")]
    [InverseProperty("BbpneoMassnahmeRegelungBveAps")]
    public virtual BbpneoMassnahmeRegelungBve BbpneoMassnahmeRegelungBveRefNavigation { get; set; } = null!;

    [ForeignKey("BstRef")]
    [InverseProperty("BbpneoMassnahmeRegelungBveAps")]
    public virtual BasisBetriebsstelle? BstRefNavigation { get; set; }
}
