using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Models;

[Table("basis_kunde", Schema = "ujbaudb")]
[Index("Id", "Kdnnr", Name = "kunde_uq_id_kdnnr", IsUnique = true)]
public partial class BasisKunde
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("kdnnr", TypeName = "character varying")]
    public string Kdnnr { get; set; } = null!;

    [Column("kbez", TypeName = "character varying")]
    public string? Kbez { get; set; }

    [Column("name", TypeName = "character varying")]
    public string? Name { get; set; }

    [Column("verkehrsart", TypeName = "character varying")]
    public string? Verkehrsart { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [Column("ist_basis_datensatz")]
    public bool? IstBasisDatensatz { get; set; }

    [InverseProperty("KundeRefNavigation")]
    public virtual ICollection<FploDokumentZug> FploDokumentZug { get; set; } = new List<FploDokumentZug>();

    [InverseProperty("KundeRefNavigation")]
    public virtual ICollection<UebDokumentZug> UebDokumentZug { get; set; } = new List<UebDokumentZug>();

    [InverseProperty("KundeRefNavigation")]
    public virtual ICollection<ZvfDokumentZug> ZvfDokumentZug { get; set; } = new List<ZvfDokumentZug>();
}
