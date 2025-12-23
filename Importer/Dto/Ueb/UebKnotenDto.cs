using System;

namespace BauFahrplanMonitor.Importer.Dto.Ueb;

public class UebKnotenDto {
    public string?   BahnhofDs100 { get; set; }
    public string?   Haltart      { get; set; }
    public string?   Ankunft      { get; set; }
    public string?   Abfahrt      { get; set; }
    public DateTime? AnkunftsZeit { get; set; }
    public DateTime? Abfahrtszeit { get; set; }
    public int       RelativLage  { get; set; } = 0;
}