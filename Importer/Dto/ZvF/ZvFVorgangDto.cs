using System.Collections.Generic;
using BauFahrplanMonitor.Importer.Dto.Shared;
using BauFahrplanMonitor.Models;

namespace BauFahrplanMonitor.Importer.Dto.ZvF;

public class ZvFVorgangDto
    : SharedVorgangDto, IExtendedVorgangDto {
    public string       Extension { get; set; } = string.Empty;
    public string       Korridor  { get; set; } = string.Empty;
    public string       KigBau    { get; set; } = string.Empty;
    public bool         IstQs     { get; set; } = false;
    public bool         IstKs     { get; set; } = false;
    public List<string> Bbmn      { get; set; } = [];

    public void ApplyTo(UjbauVorgang entity) {
        entity.Extension = Extension;
        entity.Kigbau    = KigBau;
        entity.Korridor  = Korridor;
        entity.IstKs     = IstKs;
        entity.IstQs     = IstQs;
    }
}