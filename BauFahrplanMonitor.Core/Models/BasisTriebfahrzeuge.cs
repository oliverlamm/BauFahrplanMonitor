using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Core.Models;

[Table("basis_triebfahrzeuge", Schema = "ujbaudb")]
[Index("Hauptnummer", "Unternummer", Name = "basis_triebfahrzeuge_unique", IsUnique = true)]
public partial class BasisTriebfahrzeuge
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("hauptnummer", TypeName = "character varying")]
    public string Hauptnummer { get; set; } = null!;

    [Column("unternummer")]
    public long Unternummer { get; set; }

    [Column("kennung", TypeName = "character varying")]
    public string? Kennung { get; set; }

    [Column("kennung_wert")]
    public long? KennungWert { get; set; }

    [Column("elektrifiziert")]
    public bool Elektrifiziert { get; set; }

    [Column("bezeichnung", TypeName = "character varying")]
    public string? Bezeichnung { get; set; }

    [Column("baureihenname", TypeName = "character varying")]
    public string? Baureihenname { get; set; }

    [Column("aktive_neigetechnik")]
    public bool AktiveNeigetechnik { get; set; }

    [Column("triebwagen")]
    public bool Triebwagen { get; set; }
}
