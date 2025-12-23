using System;
using System.Collections.Generic;
using System.Linq;
using BauFahrplanMonitor.Importer.Dto.ZvF;

namespace BauFahrplanMonitor.Importer.Helper;

public static class ZvFMergeHelper {
    public static void MergeZuege(ZvFDocumentDto doc) {
        var merged = new Dictionary<(long, DateOnly), ZvFZugDto>();

        foreach (var zug in doc.ZuegeRaw) {
            var key = (zug.Zugnummer, zug.Verkehrstag);

            if (!merged.TryGetValue(key, out var existing)) {
                merged[key] = zug;
                continue;
            }

            Merge(existing, zug);
        }

        doc.Zuege = merged.Values.ToList();
    }

    private static void Merge(ZvFZugDto target, ZvFZugDto source) {
        target.Zugbez    = string.IsNullOrEmpty(target.Zugbez) ? source.Zugbez : target.Zugbez;
        target.Betreiber = string.IsNullOrEmpty(target.Betreiber) ? source.Betreiber : target.Betreiber;

        target.Aenderung   ??= source.Aenderung;
        target.Regelweg    ??= source.Regelweg;
        target.Bemerkung   ??= source.Bemerkung;
        target.Bemerkungen ??= source.Bemerkungen;


        foreach (var a in from a in source.Abweichungen
                 let exists = target.Abweichungen.Any(t =>
                     t.Regelungsart == a.Regelungsart &&
                     t.AnchorRl100  == a.AnchorRl100  &&
                     t.JsonRaw      == a.JsonRaw)
                 where !exists
                 select a) {
            target.Abweichungen.Add(a);
        }

        target.Abweichungen = target.Abweichungen
            .GroupBy(a => new {
                a.Regelungsart,
                a.AnchorRl100,
                a.JsonRaw
            })
            .Select(g => g.First())
            .ToList();
    }
}