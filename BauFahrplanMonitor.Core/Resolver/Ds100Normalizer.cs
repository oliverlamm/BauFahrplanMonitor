using System.Text;

namespace BauFahrplanMonitor.Resolver;

/// <summary>
/// Hilfsklasse zur Normalisierung von DS100-Codes
/// (Betriebsstellenkennzeichen).
///
/// Verantwortlich für:
///  - Bereinigung von Rohwerten aus XML / Textfeldern
///  - Vereinheitlichung der Schreibweise vor DB-Resolve
///  - Vermeidung von fehlerhaften oder leeren DS100-Werten
///
/// NICHT verantwortlich für:
///  - Fachliche Validierung (Existenz der Betriebsstelle)
///  - Auflösung in Datenbank-IDs
///  - Kontextabhängige Interpretation
/// </summary>
/// <remarks>
/// Diese Klasse wird typischerweise verwendet:
///  - vor <see cref="Shared.ReferenceResolver.SharedReferenceResolver"/>
///  - in BusinessLogic zur Anchor-Bestimmung
///
/// Ziel ist es, alle DS100-Werte in eine
/// stabile, vergleichbare Form zu bringen.
/// </remarks>
public static class Ds100Normalizer {

    /// <summary>
    /// Bereinigt einen DS100-Code.
    /// </summary>
    /// <param name="raw">
    /// Rohwert (z. B. aus XML, Benutzerinput oder Fremdsystemen)
    /// </param>
    /// <returns>
    /// Bereinigter DS100-Code oder <c>null</c>,
    /// wenn kein sinnvoller Wert übrig bleibt.
    /// </returns>
    /// <remarks>
    /// Durchgeführte Schritte:
    ///  - Trimmen führender und folgender Whitespaces
    ///  - Zulassen nur von:
    ///      - Buchstaben
    ///      - Ziffern
    ///      - Leerzeichen
    ///  - Mehrere Whitespaces werden auf genau eines reduziert
    ///  - Sonderzeichen werden vollständig entfernt
    ///
    /// Beispiele:
    /// <code>
    /// "  THB "        → "THB"
    /// "B E R  "       → "B E R"
    /// "KÖLN!"         → "K LN"
    /// "***"           → null
    /// </code>
    /// </remarks>
    public static string? Clean(string? raw) {

        if( string.IsNullOrWhiteSpace(raw) )
            return null;

        var trimmed = raw.Trim();

        var sb = new StringBuilder(trimmed.Length);
        var lastSpace = false;

        foreach( var ch in trimmed ) {

            if( char.IsLetterOrDigit(ch) ) {
                sb.Append(ch);
                lastSpace = false;
            }
            else if( char.IsWhiteSpace(ch) ) {
                // Mehrere Whitespaces hintereinander vermeiden
                if( lastSpace )
                    continue;

                sb.Append(' ');
                lastSpace = true;
            }
            // alle anderen Zeichen (Sonderzeichen) werden ignoriert
        }

        // Sicherheit: führende / folgende Spaces entfernen
        var result = sb.ToString().Trim();

        return string.IsNullOrEmpty(result)
            ? null
            : result;
    }
}
