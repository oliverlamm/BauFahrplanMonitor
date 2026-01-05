using System;
using System.Collections.Generic;
using BauFahrplanMonitor.Importer.Xml;

namespace BauFahrplanMonitor.Importer.Dto.Shared;

/// <summary>
/// Gemeinsames Zug-DTO für alle Importer.
///
/// Repräsentiert die fachlichen Kerndaten eines Zuges
/// (Zugnummer, Verkehrstag, Regelweg, Referenzen, Zusatzmerkmale).
///
/// Dieses DTO bildet die zentrale Übergabestruktur zwischen:
///  - importer-spezifischen Normalizern
///  - BusinessLogic
///  - Upsert- und ReferenceResolver-Schicht
/// </summary>
/// <remarks>
/// <para>
/// Das <see cref="SharedZugDto"/> ist bewusst:
/// <list type="bullet">
///   <item>importerübergreifend</item>
///   <item>referenzfähig (DB-IDs)</item>
///   <item>erweiterbar um Raw-XSD-Objekte</item>
/// </list>
/// </para>
///
/// Raw-Objekte aus der XSD werden hier bewusst
/// <b>noch nicht</b> aufgelöst, sondern später
/// im jeweiligen TrainImporter verarbeitet.
/// </remarks>
public class SharedZugDto {

    // ==========================================================
    // IDENTITÄT
    // ==========================================================

    /// <summary>
    /// Zugnummer.
    /// </summary>
    /// <remarks>
    /// Fachlicher Primärschlüssel eines Zuges
    /// in Kombination mit <see cref="Verkehrstag"/>.
    /// </remarks>
    public long Zugnummer { get; set; }

    /// <summary>
    /// Verkehrstag des Zuges.
    /// </summary>
    /// <remarks>
    /// Gibt den Kalendertag an, an dem der Zug verkehrt.
    /// </remarks>
    public DateOnly Verkehrstag { get; set; }

    /// <summary>
    /// Zugbezeichnung (z. B. ICE 123).
    /// </summary>
    public string Zugbez { get; set; } = "";

    /// <summary>
    /// Betreiber des Zuges.
    /// </summary>
    /// <remarks>
    /// Typischerweise EVU oder Organisation.
    /// Als <c>init</c>-Property modelliert, da
    /// sie sich nach Initialisierung nicht mehr ändern soll.
    /// </remarks>
    public string Betreiber { get; set; } = "";

    // ==========================================================
    // REGELWEG (ROHDATEN)
    // ==========================================================

    /// <summary>
    /// DS100-Code der Abgangs-Betriebsstelle.
    /// </summary>
    /// <remarks>
    /// Rohwert; sollte vor Referenzauflösung
    /// über den <see cref="Helper.Ds100Normalizer"/>
    /// bereinigt werden.
    /// </remarks>
    public string AbgangDs100 { get; set; } = "";

    /// <summary>
    /// DS100-Code der Ziel-Betriebsstelle.
    /// </summary>
    public string ZielDs100 { get; set; } = "";

    /// <summary>
    /// Liniennummer des Regelwegs.
    /// </summary>
    public string LinienNr { get; set; } = "";

    // ==========================================================
    // ZVF / ÜB-SPEZIFISCHE FELDER
    // ==========================================================

    /// <summary>
    /// Tageswechsel-Indikator.
    /// </summary>
    /// <remarks>
    /// Gibt an, ob der Zug:
    /// <list type="bullet">
    ///   <item>-1 → am Vortag beginnt</item>
    ///   <item>0  → am Verkehrstag verkehrt</item>
    ///   <item>1  → am Folgetag endet</item>
    /// </list>
    /// </remarks>
    public int Tageswechsel { get; set; }

    /// <summary>
    /// Liste freier Bemerkungen zum Zug.
    /// </summary>
    public string Bemerkungen { get; set; } = "";

    /// <summary>
    /// KLV-Kennzeichen.
    /// </summary>
    public string Klv { get; set; } = "";

    /// <summary>
    /// SKL-Kennzeichen.
    /// </summary>
    public string Skl { get; set; } = "";

    /// <summary>
    /// BZA-Kennzeichen.
    /// </summary>
    public string Bza { get; set; } = "";

    // ==========================================================
    // RAW-XSD-OBJEKTE
    // ==========================================================

    /// <summary>
    /// Raw-Regelweg-Objekt aus der ZvF-XML-XSD.
    /// </summary>
    /// <remarks>
    /// Wird bewusst unverändert gehalten
    /// und später im TrainImporter ausgewertet.
    /// </remarks>
    public ZvFExportBaumassnahmenBaumassnahmeZuegeZugRegelweg? Regelweg { get; set; }

    /// <summary>
    /// Raw-Bemerkungsobjekt aus der ZvF-XML-XSD.
    /// </summary>
    /// <remarks>
    /// Dient als Quelle für strukturierte
    /// oder formatierte Bemerkungen.
    /// </remarks>
    public ZvFExportBaumassnahmenBaumassnahmeZuegeZugBemerkung? Bemerkung { get; set; }
}
