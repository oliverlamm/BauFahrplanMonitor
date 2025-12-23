using System;
using BauFahrplanMonitor.Importer.Helper;

namespace BauFahrplanMonitor.Importer.Dto.Shared {
    /// <summary>
    /// Gemeinsames Strecken-DTO für alle Importer.
    ///
    /// Repräsentiert die fachlichen Basisinformationen
    /// einer von einer Maßnahme betroffenen Strecke.
    /// </summary>
    /// <remarks>
    /// Dieses DTO ist importerübergreifend und wird verwendet für:
    ///  - ZvF
    ///  - ÜB
    ///  - FPLO
    ///  - BBPNeo
    ///
    /// Es enthält ausschließlich **streckenbezogene Metadaten**
    /// und keine zug- oder dokument-spezifischen Details.
    ///
    /// Die Felder sind bewusst größtenteils nullable,
    /// da nicht jeder Import alle Informationen liefert.
    /// </remarks>
    public class SharedStreckeDto {
        /// <summary>
        /// Grund der Maßnahme auf dieser Strecke.
        /// </summary>
        /// <remarks>
        /// Freitext oder Code, abhängig vom Quellsystem
        /// (z. B. Baugrund, Instandhaltung, Sperrung).
        /// </remarks>
        public string? Grund { get; set; }

        /// <summary>
        /// VZG-Nummer der Strecke.
        /// </summary>
        /// <remarks>
        /// Wird typischerweise später über den
        /// <c>SharedReferenceResolver</c>
        /// in eine Datenbank-Referenz aufgelöst.
        /// </remarks>
        public string? Vzg { get; set; } = "";

        /// <summary>
        /// Kennzeichnung des Exports oder der Exportquelle.
        /// </summary>
        /// <remarks>
        /// Kann z. B. Informationen über:
        ///  - Exporttyp
        ///  - Exportversion
        ///  - Herkunftssystem
        /// enthalten.
        /// </remarks>
        public string? Export { get; set; }

        /// <summary>
        /// Bezeichnung der Maßnahme auf dieser Strecke.
        /// </summary>
        /// <remarks>
        /// Wird häufig als Kurzbeschreibung oder Titel
        /// der Maßnahme verwendet.
        /// </remarks>
        public string? Massnahme { get; set; }

        /// <summary>
        /// Start-Betriebsstelle (DS100) der betroffenen Strecke.
        /// </summary>
        /// <remarks>
        /// Rohwert; sollte vor DB-Resolve über
        /// <see cref="Ds100Normalizer"/>
        /// bereinigt werden.
        /// </remarks>
        public string? StartBst { get; set; }

        /// <summary>
        /// End-Betriebsstelle (DS100) der betroffenen Strecke.
        /// </summary>
        /// <remarks>
        /// Rohwert; sollte vor DB-Resolve über
        /// <see cref="Ds100Normalizer"/>
        /// bereinigt werden.
        /// </remarks>
        public string? EndBst { get; set; }

        /// <summary>
        /// Betriebsweise während der Maßnahme.
        /// </summary>
        /// <remarks>
        /// Beispiele:
        ///  - eingleisig
        ///  - gesperrt
        ///  - Ersatzverkehr
        /// </remarks>
        public string? Betriebsweise { get; set; }

        /// <summary>
        /// Geplanter Beginn der Baumaßnahme.
        /// </summary>
        public DateTime? Baubeginn { get; set; }

        /// <summary>
        /// Geplantes Ende der Baumaßnahme.
        /// </summary>
        public DateTime? Bauende { get; set; }

        /// <summary>
        /// Freitext zur Unterbrechung oder Einschränkung.
        /// </summary>
        /// <remarks>
        /// Wird verwendet für:
        ///  - Zeiträume mit Unterbrechungen
        ///  - Sonderregelungen
        ///  - erläuternde Hinweise
        /// </remarks>
        public bool ZeitraumUnterbrochen { get; set; } = false;
    }
}