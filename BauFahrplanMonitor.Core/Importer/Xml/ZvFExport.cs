using System.ComponentModel;
using System.Xml.Serialization;

namespace BauFahrplanMonitor.Core.Importer.Xml;

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
[XmlRoot(Namespace = "", IsNullable = false, ElementName = "zvfexport")]
public class ZvFExport {
    [XmlElement(ElementName = "header")]
    public ZvFExportHeader? Header { get; set; }

    [XmlElement(ElementName = "baumassnahmen")]
    public ZvFExportBaumassnahmen? Baumassnahmen { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public class ZvFExportHeader {
    // Gemeinsames Feld, um Empfaengerlist & Empfaengerliste wie bisher zu koppeln
    private string[]? _empfaengerliste;

    [XmlElement(ElementName = "timestamp")]
    public DateTime Timestamp { get; set; }

    [XmlElement(ElementName = "filename")]
    public string? Filename { get; set; }

    [XmlElement(ElementName = "version_prg_zvf")]
    public string? VersionPrgZvf { get; set; }

    [XmlElement(ElementName = "endStueckZvF")]
    public string? EndStueckZvF { get; set; }

    [XmlElement(ElementName = "sender")]
    public ZvFExportHeaderSender? Sender { get; set; }

    [XmlArrayItem("empfaenger", IsNullable = false)]
    public string[]? Empfaengerlist {
        get => _empfaengerliste;
        set => _empfaengerliste = value;
    }

    [XmlArrayItem("empfaenger", IsNullable = false)]
    public string[]? Empfaengerliste {
        get => _empfaengerliste;
        set => _empfaengerliste = value;
    }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public class ZvFExportHeaderSender {
    [XmlElement(ElementName = "name")]
    public string? Name { get; set; }

    [XmlElement(ElementName = "vorname")]
    public string? Vorname { get; set; }

    [XmlElement(ElementName = "kuerzel")]
    public string? Kuerzel { get; set; }

    [XmlElement(ElementName = "abteilung")]
    public string? Abteilung { get; set; }

    [XmlElement(ElementName = "strasse")]
    public string? Strasse { get; set; }

    [XmlElement(ElementName = "plz")]
    public string? Plz { get; set; }

    [XmlElement(ElementName = "ort")]
    public string? Ort { get; set; }

    [XmlElement(ElementName = "email")]
    public string? Email { get; set; }

    [XmlElement(ElementName = "telefon")]
    public string? Telefon { get; set; }

    [XmlElement(ElementName = "telefon_intern")]
    public string? TelefonIntern { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public class BetriebsstelleDS100 {
    [XmlAttribute(AttributeName = "ds100")]
    public string? Ds100 { get; set; }

    [XmlText]
    public string? Value { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public class ZvFExportBaumassnahmen {
    [XmlElement(ElementName = "baumassnahme")]
    public ZvFExportBaumassnahmenBaumassnahme? Baumassnahme { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public class ZvFExportBaumassnahmenBaumassnahme {
    [XmlElement(ElementName = "masterniederlassung")]
    public string? Masterniederlassung { get; set; }

    [XmlElement(ElementName = "zvfid")]
    public string? Zvfid { get; set; }

    [XmlElement(ElementName = "baumassnahmenart")]
    public string? Baumassnahmenart { get; set; }

    [XmlElement(ElementName = "qsbaumassnahme")]
    public string? Qsbaumassnahme { get; set; }

    [XmlElement(ElementName = "korridor")]
    public string? Korridor { get; set; }

    [XmlElement(ElementName = "kigbau")]
    public string? Kigbau { get; set; }

    [XmlElement(ElementName = "kennung")]
    public string? Kennung { get; set; }

    [XmlElement(ElementName = "extension")]
    public string? Extension { get; set; }

    [XmlArray("bbpliste")]
    [XmlArrayItem("bbp", IsNullable = false, ElementName = "bbpliste")]
    public string[]? Bbpliste { get; set; }

    [XmlElement(ElementName = "master_fplo")]
    public int MasterFplo { get; set; }

    [XmlElement(ElementName = "festgelegtSPFV")]
    public int FestgelegtSPFV { get; set; }

    [XmlElement(ElementName = "festgelegtSPNV")]
    public int FestgelegtSPNV { get; set; }

    [XmlElement(ElementName = "festgelegtSGV")]
    public int FestgelegtSGV { get; set; }

    [XmlElement(DataType = "date", ElementName = "antwort")]
    public DateTime? Antwort { get; set; }

    [XmlElement(DataType = "date", ElementName = "baudatevon")]
    public DateTime? BauDatumVon { get; set; }

    [XmlElement(DataType = "date", ElementName = "baudatebis")]
    public DateTime? BauDatumBis { get; set; }

    [XmlElement(ElementName = "endStueckZvf")]
    public int EndStueckZvf { get; set; }

    [XmlElement(ElementName = "version")]
    public ZvFExportBaumassnahmenBaumassnahmeVersion? Version { get; set; }

    [XmlArray("streckenabschnitte")]
    [XmlArrayItem(ElementName = "strecke", IsNullable = false)]
    public ZvFExportBaumassnahmenBaumassnahmeStrecke[]? Streckenabschnitte { get; set; }

    [XmlElement(ElementName = "gueltigkeit_fplo")]
    public ZvFExportBaumassnahmenBaumassnahmeGueltigkeitFplo? GueltigkeitFplo { get; set; }

    [XmlArray("allgregelungen")]
    [XmlArrayItem(ElementName = "allgregelung", IsNullable = false)]
    public string[]? Allgregelungen { get; set; }

    [XmlArray("fplonr")]
    [XmlArrayItem(ElementName = "niederlassung", IsNullable = false)]
    public ZvFExportBaumassnahmenBaumassnahmeNiederlassung[]? Fplonr { get; set; }

    [XmlElement(ElementName = "zuege")]
    public ZvFExportBaumassnahmenBaumassnahmeZuege? Zuege { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public class ZvFExportBaumassnahmenBaumassnahmeGueltigkeitFplo {
    [XmlElement(DataType = "date", ElementName = "beginn")]
    public DateTime Beginn { get; set; }

    [XmlElement(DataType = "date", ElementName = "ende")]
    public DateTime Ende { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public class ZvFExportBaumassnahmenBaumassnahmeNiederlassung {
    [XmlAttribute(AttributeName = "beteiligt")]
    public int Beteiligt { get; set; }

    [XmlAttribute(AttributeName = "fplo")]
    public string? Fplo { get; set; }

    [XmlText]
    public string? Value { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public class ZvFExportBaumassnahmenBaumassnahmeStrecke {
    [XmlElement(ElementName = "grund")]
    public string? Grund { get; set; }

    [XmlArray(ElementName = "vzgliste")]
    [XmlArrayItem(ElementName = "vzg", IsNullable = false)]
    public int[]? VzGListe { get; set; }

    [XmlElement(ElementName = "export")]
    public string? Export { get; set; }

    [XmlElement(ElementName = "massnahme")]
    public string? Massnahme { get; set; }

    [XmlElement(ElementName = "startbst")]
    public string? Startbst { get; set; }

    [XmlElement(ElementName = "endbst")]
    public string? Endbst { get; set; }

    [XmlElement(ElementName = "betriebsweise")]
    public string? Betriebsweise { get; set; }

    [XmlElement(ElementName = "baubeginn")]
    public DateTime Baubeginn { get; set; }

    [XmlElement(ElementName = "bauende")]
    public DateTime Bauende { get; set; }

    [XmlElement(ElementName = "zeitraum_unterbrochen")]
    public string? ZeitraumUnterbrochen { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public class ZvFExportBaumassnahmenBaumassnahmeVersion {
    [XmlElement(ElementName = "titel")]
    public string? Titel { get; set; }

    [XmlElement(ElementName = "formular")]
    public string? Formular { get; set; }

    [XmlElement(ElementName = "major")]
    public int Major { get; set; }

    [XmlElement(ElementName = "minor")]
    public int Minor { get; set; }

    [XmlElement(ElementName = "sub")]
    public int Sub { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public class ZvFExportBaumassnahmenBaumassnahmeZuege {
    [XmlElement(ElementName = "zug")]
    public ZvFExportBaumassnahmenBaumassnahmeZuegeZug[]? Zug { get; set; }

    [XmlElement(ElementName = "sev")]
    public ZvFExportBaumassnahmenBaumassnahmeZuegeSev[]? Sev { get; set; }

    [XmlElement(ElementName = "zugparameter")]
    public ZvFExportBaumassnahmenBaumassnahmeZuegeZugparameter[]? Zugparameter { get; set; }

    [XmlElement(ElementName = "zurueckgehalten")]
    public ZvFExportBaumassnahmenBaumassnahmeZuegeZurueckgehalten[]? Zurueckgehalten { get; set; }

    [XmlElement(ElementName = "haltausfall")]
    public ZvFExportBaumassnahmenBaumassnahmeZuegeHaltausfall[]? Haltausfall { get; set; }

    [XmlArray("Entfallen")]
    [XmlArrayItem("zug", IsNullable = false)]
    public ZvFExportBaumassnahmenBaumassnahmeZuegeEntfalleneZuege[]? Entfallen { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public class ZvFExportBaumassnahmenBaumassnahmeZuegeEntfalleneZuege {
    [XmlAttribute(AttributeName = "verkehrstag")]
    public string? Verkehrstag { get; set; }

    [XmlAttribute(AttributeName = "zugnr")]
    public int Zugnr { get; set; }

    [XmlAttribute(AttributeName = "zugbez")]
    public string? Zugbez { get; set; }

    [XmlAttribute(AttributeName = "regelungsartalt")]
    public string? RegelungsArtalt { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public class ZvFExportBaumassnahmenBaumassnahmeZuegeHaltausfall {
    [XmlElement(ElementName = "zurueckgehalten")]
    public ZvFExportBaumassnahmenBaumassnahmeZuegeHaltausfallZurueckgehalten? Zurueckgehalten { get; set; }

    [XmlElement(ElementName = "ausfallender_halt")]
    public BetriebsstelleDS100? AusfallenderHalt { get; set; }

    [XmlElement(ElementName = "Ersatzhalt")]
    public BetriebsstelleDS100? Ersatzhalt { get; set; }

    [XmlAttribute(AttributeName = "zugnr")]
    public int Zugnr { get; set; }

    [XmlAttribute(DataType = "date", AttributeName = "verkehrstag")]
    public DateTime Verkehrstag { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public class ZvFExportBaumassnahmenBaumassnahmeZuegeHaltausfallZurueckgehalten {
    [XmlElement(ElementName = "ab_bst")]
    public BetriebsstelleDS100? AbBst { get; set; }

    [XmlElement(ElementName = "zurueckhalten_bis")]
    public string? ZurueckhaltenBis { get; set; }

    [XmlAttribute(AttributeName = "zugnr")]
    public int Zugnr { get; set; }

    [XmlAttribute(DataType = "date", AttributeName = "verkehrstag")]
    public DateTime Verkehrstag { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public class ZvFExportBaumassnahmenBaumassnahmeZuegeSev {
    [XmlElement(ElementName = "ausfall_von")]
    public BetriebsstelleDS100? AusfallVon { get; set; }

    [XmlElement(ElementName = "ausfall_bis")]
    public BetriebsstelleDS100? AusfallBis { get; set; }

    [XmlElement(ElementName = "neuer_fahrplan")]
    public int NeuerFahrplan { get; set; }

    [XmlElement(ElementName = "ersatzzug")]
    public ZvFExportBaumassnahmenBaumassnahmeZuegeSevErsatzzug? Ersatzzug { get; set; }

    [XmlAttribute(AttributeName = "zugnr")]
    public int Zugnr { get; set; }

    [XmlAttribute(AttributeName = "verspaetet")]
    public int Verspaetet { get; set; }

    [XmlAttribute(AttributeName = "im_plan")]
    public int ImPlan { get; set; }

    [XmlAttribute(DataType = "date", AttributeName = "verkehrstag")]
    public DateTime Verkehrstag { get; set; }

    [XmlAttribute(AttributeName = "startbf")]
    public string? Startbf { get; set; }

    [XmlAttribute(AttributeName = "zielbf")]
    public string? Zielbf { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public class ZvFExportBaumassnahmenBaumassnahmeZuegeSevErsatzzug {
    [XmlElement(ElementName = "im_plan")]
    public string? ImPlan { get; set; }

    [XmlElement(ElementName = "neuer_fahrplan")]
    public int NeuerFahrplan { get; set; }

    [XmlAttribute(AttributeName = "verspaetet")]
    public int Verspaetet { get; set; }

    [XmlAttribute(AttributeName = "zugnr")]
    public int Zugnr { get; set; }

    [XmlAttribute(DataType = "date", AttributeName = "verkehrstag")]
    public DateTime Verkehrstag { get; set; }

    [XmlAttribute(AttributeName = "startbf")]
    public string? Startbf { get; set; }

    [XmlAttribute(AttributeName = "zielbf")]
    public string? Zielbf { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public class ZvFExportBaumassnahmenBaumassnahmeZuegeZug {
    [XmlElement(ElementName = "regelweg")]
    public ZvFExportBaumassnahmenBaumassnahmeZuegeZugRegelweg? Regelweg { get; set; }

    [XmlElement(ElementName = "abweichung")]
    public ZvFExportBaumassnahmenBaumassnahmeZuegeZugAbweichung? Abweichung { get; set; }

    [XmlAttribute(AttributeName = "firstbst")]
    public string? FirstBst { get; set; }

    [XmlAttribute(AttributeName = "aenderung")]
    public string? Aenderung { get; set; }

    [XmlAttribute(AttributeName = "bedarf")]
    public int Bedarf { get; set; }

    [XmlAttribute(DataType = "date", AttributeName = "verkehrstag")]
    public DateTime Verkehrstag { get; set; }

    [XmlAttribute(AttributeName = "betreiber")]
    public string? Betreiber { get; set; }

    [XmlAttribute(AttributeName = "zugnr")]
    public int Zugnr { get; set; }

    [XmlAttribute(AttributeName = "zugbez")]
    public string? Zugbez { get; set; }

    [XmlAttribute(AttributeName = "sonder")]
    public int Sonder { get; set; }

    [XmlIgnore]
    public bool SonderSpecified { get; set; }

    [XmlAttribute(AttributeName = "zuggat")]
    public decimal Zuggat { get; set; }

    [XmlAttribute(AttributeName = "ausfall")]
    public int Ausfall { get; set; }

    [XmlIgnore]
    public bool AusfallSpecified { get; set; }

    [XmlAttribute(AttributeName = "vorplan")]
    public int Vorplan { get; set; }

    [XmlIgnore]
    public bool VorplanSpecified { get; set; }

    [XmlAttribute(AttributeName = "fplo_abschnitt")]
    public string? FploAbschnitt { get; set; }

    [XmlAttribute(AttributeName = "tageswechsel")]
    public string? Tageswechsel { get; set; }

    [XmlAttribute(AttributeName = "sicherheitsrelevanterzug")]
    public string? Sicherheitsrelevanterzug { get; set; }

    [XmlIgnore]
    public bool SicherheitsrelevanterzugSpecified { get; set; }

    [XmlAttribute(AttributeName = "lauterzug")]
    public string? Lauterzug { get; set; }

    [XmlIgnore]
    public bool LauterzugSpecified { get; set; }

    [XmlAttribute(AttributeName = "vmax")]
    public string? Vmax { get; set; }

    [XmlAttribute(AttributeName = "tfz")]
    public string? Tfz { get; set; }

    [XmlAttribute(AttributeName = "last")]
    public string? Last { get; set; }

    [XmlAttribute(AttributeName = "laenge")]
    public string? Laenge { get; set; }

    [XmlAttribute(AttributeName = "brems")]
    public string? Brems { get; set; }

    [XmlAttribute(AttributeName = "ebula")]
    public string? Ebula { get; set; }

    [XmlIgnore]
    public bool EbulaSpecified { get; set; }

    [XmlAttribute(AttributeName = "skl")]
    public string? Skl { get; set; }

    [XmlAttribute(AttributeName = "klv")]
    public string? Klv { get; set; }

    [XmlAttribute(AttributeName = "bza")]
    public string? Bza { get; set; }

    [XmlElement(ElementName = "bemerkung")]
    public ZvFExportBaumassnahmenBaumassnahmeZuegeZugBemerkung? Bemerkung { get; set; }

    [XmlElement(ElementName = "fahrplan")]
    public ZvFExportBaumassnahmenBaumassnahmeZuegeZugFahrplan? Fahrplan { get; set; }

    [XmlElement(ElementName = "knotenzeiten")]
    public ZvFExportBaumassnahmenBaumassnahmeZuegeZugKnotenzeiten? Knotenzeiten { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public class ZvFExportBaumassnahmenBaumassnahmeZuegeZugAbweichendeZugparameter {
    public int Last { get; set; }
    public int Laenge { get; set; }
    public int KV { get; set; }
    public int Tfz { get; set; }
    public int Bremsstellung { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public class ZvFExportBaumassnahmenBaumassnahmeZuegeZugAbweichung {
    [XmlArray(ElementName = "regelungsliste")]
    [XmlArrayItem("regelung", IsNullable = false)]
    public ZvfexportBaumassnahmenBaumassnahmeZuegeZugAbweichungRegelung[]? Regelungsliste { get; set; }

    [XmlElement(ElementName = "umleitung")]
    public string? Umleitung { get; set; }

    [XmlArray(ElementName = "umleitweg")]
    [XmlArrayItem("ds100", IsNullable = false)]
    public string[]? Umleitweg { get; set; }

    [XmlElement(ElementName = "vorplanab")]
    public BetriebsstelleDS100? Vorplanab { get; set; }

    [XmlElement(ElementName = "verspaetung")]
    public string? Verspaetung { get; set; }

    [XmlElement(ElementName = "verspaetungab")]
    public BetriebsstelleDS100? Verspaetungab { get; set; }

    [XmlArray(ElementName = "haltliste")]
    [XmlArrayItem("halt", IsNullable = false)]
    public ZvFExportBaumassnahmenBaumassnahmeZuegeZugAbweichungHalt[]? Haltliste { get; set; }

    [XmlElement(ElementName = "ausfallvon")]
    public BetriebsstelleDS100? Ausfallvon { get; set; }

    [XmlElement(ElementName = "ausfallbis")]
    public BetriebsstelleDS100? Ausfallbis { get; set; }

    [XmlAttribute(AttributeName = "art")]
    public string? Art { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public class ZvFExportBaumassnahmenBaumassnahmeZuegeZugAbweichungHalt {
    [XmlElement(ElementName = "folge")]
    public int Folge { get; set; }

    [XmlElement(ElementName = "art")]
    public int Art { get; set; }

    [XmlElement(ElementName = "ausfall")]
    public BetriebsstelleDS100? Ausfall { get; set; }

    [XmlElement(ElementName = "ersatz")]
    public BetriebsstelleDS100? Ersatz { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public class ZvfexportBaumassnahmenBaumassnahmeZuegeZugAbweichungRegelung {
    [XmlElement(ElementName = "art")]
    public string? Art { get; set; }

    [XmlElement(ElementName = "gilt_in")]
    public ZvfexportBaumassnahmenBaumassnahmeZuegeZugAbweichungRegelungslisteRegelungGilt_in? GiltIn { get; set; }

    [XmlElement(ElementName = "text")]
    public string? Text { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public class ZvfexportBaumassnahmenBaumassnahmeZuegeZugAbweichungRegelungslisteRegelungGilt_in {
    [XmlAttribute(AttributeName = "ds100")]
    public string? Ds100 { get; set; }

    [XmlText]
    public string? Value { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public class ZvFExportBaumassnahmenBaumassnahmeZuegeZugBemerkung {
    [XmlElement("bemerk")]
    public ZvFExportBaumassnahmenBaumassnahmeZuegeZugBemerkungBemerk[]? Bemerk { get; set; }

    [XmlText]
    public string[]? Text { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public class ZvFExportBaumassnahmenBaumassnahmeZuegeZugBemerkungBemerk {
    [XmlAttribute(AttributeName = "lfd")]
    public int Lfd { get; set; }

    [XmlText]
    public string? Value { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public class ZvFExportBaumassnahmenBaumassnahmeZuegeZugFahrplan {
    [XmlElement(ElementName = "fahrplanzeit")]
    public ZvFExportBaumassnahmenBaumassnahmeZuegeZugFahrplanFahrplanzeit[]? Fahrplanzeit { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public class ZvFExportBaumassnahmenBaumassnahmeZuegeZugFahrplanFahrplanzeit {
    [XmlElement(ElementName = "lfd_nr")]
    public int LfdNr { get; set; }

    [XmlElement(ElementName = "bahnhof")]
    public string? Bahnhof { get; set; }

    [XmlElement(ElementName = "haltart")]
    public string? Haltart { get; set; }

    [XmlElement(ElementName = "ankunft")]
    public string? Ankunft { get; set; }

    [XmlElement(ElementName = "abfahrt")]
    public string? Abfahrt { get; set; }

    [XmlElement(ElementName = "tagwechsel")]
    public int Tagwechsel { get; set; }

    [XmlElement(ElementName = "strecke")]
    public string? Strecke { get; set; }

    [XmlElement(ElementName = "bemerkung")]
    public string? Bemerkung { get; set; }

    [XmlElement(ElementName = "bfpl")]
    public ZvFExportBaumassnahmenBaumassnahmeZuegeZugFahrplanFahrplanzeitBfpl? Bfpl { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public class ZvFExportBaumassnahmenBaumassnahmeZuegeZugFahrplanFahrplanzeitBfpl {
    [XmlAttribute(AttributeName = "ebulavglzug")]
    public string? Ebulavglzug { get; set; }

    [XmlAttribute(AttributeName = "ebulavglmbr")]
    public string? Ebulavglmbr { get; set; }

    [XmlAttribute(AttributeName = "ebulavglbrs")]
    public string? Ebulavglbrs { get; set; }

    [XmlAttribute(AttributeName = "efplh")]
    public string? Efplh { get; set; }

    [XmlAttribute(AttributeName = "efpls")]
    public string? Efpls { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public class ZvFExportBaumassnahmenBaumassnahmeZuegeZugKnotenzeiten {
    [XmlElement("knotenzeit")]
    public ZvFExportBaumassnahmenBaumassnahmeZuegeZugKnotenzeitenKnotenzeit[]? Knotenzeit { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public class ZvFExportBaumassnahmenBaumassnahmeZuegeZugKnotenzeitenKnotenzeit {
    [XmlElement(ElementName = "bahnhof")]
    public string? Bahnhof { get; set; }

    [XmlElement(ElementName = "haltart")]
    public string? Haltart { get; set; }

    [XmlElement(ElementName = "ankunft")]
    public string? Ankunft { get; set; }

    [XmlElement(ElementName = "abfahrt")]
    public string? Abfahrt { get; set; }

    [XmlElement(ElementName = "relativlage")]
    public string? Relativlage { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public class ZvFExportBaumassnahmenBaumassnahmeZuegeZugparameter {
    [XmlElement(ElementName = "wirkt_ab_bst")]
    public BetriebsstelleDS100? WirktAbBst { get; set; }

    [XmlElement(ElementName = "wirkt_bis_bst")]
    public BetriebsstelleDS100? WirktBisBst { get; set; }

    [XmlElement(ElementName = "art")]
    public string? Art { get; set; }

    [XmlElement(ElementName = "wert")]
    public string? Wert { get; set; }

    [XmlAttribute(AttributeName = "zugnr")]
    public int Zugnr { get; set; }

    [XmlAttribute(DataType = "date", AttributeName = "verkehrstag")]
    public DateTime Verkehrstag { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public class ZvFExportBaumassnahmenBaumassnahmeZuegeZugRegelweg {
    [XmlElement(ElementName = "liniennr")]
    public string? LinienNr { get; set; }

    [XmlElement(ElementName = "abgangsbahnhof")]
    public BetriebsstelleDS100? Abgangsbahnhof { get; set; }

    [XmlElement(ElementName = "zielbahnhof")]
    public BetriebsstelleDS100? Zielbahnhof { get; set; }
}

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public class ZvFExportBaumassnahmenBaumassnahmeZuegeZurueckgehalten {
    [XmlElement(ElementName = "ab_bst")]
    public BetriebsstelleDS100? AbBst { get; set; }

    [XmlElement(ElementName = "zurueckhalten_bis")]
    public string? ZurueckhaltenBis { get; set; }

    [XmlAttribute(AttributeName = "zugnr")]
    public int Zugnr { get; set; }

    [XmlAttribute(DataType = "date", AttributeName = "verkehrstag")]
    public DateTime Verkehrstag { get; set; }
}
