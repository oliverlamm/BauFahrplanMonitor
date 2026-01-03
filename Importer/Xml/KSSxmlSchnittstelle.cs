#nullable disable

namespace BauFahrplanMonitor.Importer.Xml;

// HINWEIS: Für den generierten Code ist möglicherweise mindestens .NET Framework 4.5 oder .NET Core/Standard 2.0 erforderlich.
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
[System.Xml.Serialization.XmlRoot( "KSS", Namespace = "", IsNullable = false )]
public class KSSxmlSchnittstelle {
    private KSSHeader headerField;

    private KSSEntryIndexTrassen[] indexTrassenField;

    private KSSRailml railmlField;

    private decimal versionField;

    /// <remarks/>
    public KSSHeader header {
        get { return this.headerField; }
        set { this.headerField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItem( "entryIndexTrassen", IsNullable = false )]
    public KSSEntryIndexTrassen[] indexTrassen {
        get { return this.indexTrassenField; }
        set { this.indexTrassenField = value; }
    }

    /// <remarks/>
    public KSSRailml railml {
        get { return this.railmlField; }
        set { this.railmlField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public decimal version {
        get { return this.versionField; }
        set { this.versionField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSHeader {
    private KSSHeaderDescription descriptionField;

    private string userAbbreviationField;

    private System.DateTime timeOfBuildField;

    private KSSHeaderSpurplanVersionDescription spurplanVersionDescriptionField;

    private KSSHeaderGfdiVersionDescription gfdiVersionDescriptionField;

    private string establishmentField;

    /// <remarks/>
    public KSSHeaderDescription description {
        get { return this.descriptionField; }
        set { this.descriptionField = value; }
    }

    /// <remarks/>
    public string userAbbreviation {
        get { return this.userAbbreviationField; }
        set { this.userAbbreviationField = value; }
    }

    /// <remarks/>
    public System.DateTime timeOfBuild {
        get { return this.timeOfBuildField; }
        set { this.timeOfBuildField = value; }
    }

    /// <remarks/>
    public KSSHeaderSpurplanVersionDescription spurplanVersionDescription {
        get { return this.spurplanVersionDescriptionField; }
        set { this.spurplanVersionDescriptionField = value; }
    }

    /// <remarks/>
    public KSSHeaderGfdiVersionDescription gfdiVersionDescription {
        get { return this.gfdiVersionDescriptionField; }
        set { this.gfdiVersionDescriptionField = value; }
    }

    /// <remarks/>
    public string establishment {
        get { return this.establishmentField; }
        set { this.establishmentField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSHeaderDescription {
    private KSSHeaderDescriptionScheduleVersion scheduleVersionField;

    private string shortDescriptionField;

    /// <remarks/>
    public KSSHeaderDescriptionScheduleVersion scheduleVersion {
        get { return this.scheduleVersionField; }
        set { this.scheduleVersionField = value; }
    }

    /// <remarks/>
    public string shortDescription {
        get { return this.shortDescriptionField; }
        set { this.shortDescriptionField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSHeaderDescriptionScheduleVersion {
    private string modeField;

    private string nameField;

    private int numberField;

    private KSSHeaderDescriptionScheduleVersionValidity validityField;

    private int yearTimetableField;

    private object rectifyingNumberField;

    private string classField;

    private System.DateTime timeOfBuildField;

    private string statusField;

    /// <remarks/>
    public string mode {
        get { return this.modeField; }
        set { this.modeField = value; }
    }

    /// <remarks/>
    public string name {
        get { return this.nameField; }
        set { this.nameField = value; }
    }

    /// <remarks/>
    public int number {
        get { return this.numberField; }
        set { this.numberField = value; }
    }

    /// <remarks/>
    public KSSHeaderDescriptionScheduleVersionValidity validity {
        get { return this.validityField; }
        set { this.validityField = value; }
    }

    /// <remarks/>
    public int yearTimetable {
        get { return this.yearTimetableField; }
        set { this.yearTimetableField = value; }
    }

    /// <remarks/>
    public object rectifyingNumber {
        get { return this.rectifyingNumberField; }
        set { this.rectifyingNumberField = value; }
    }

    /// <remarks/>
    public string @class {
        get { return this.classField; }
        set { this.classField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElement( DataType = "date" )]
    public System.DateTime timeOfBuild {
        get { return this.timeOfBuildField; }
        set { this.timeOfBuildField = value; }
    }

    /// <remarks/>
    public string status {
        get { return this.statusField; }
        set { this.statusField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSHeaderDescriptionScheduleVersionValidity {
    private System.DateTime validFromField;

    private System.DateTime validToField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElement( DataType = "date" )]
    public System.DateTime validFrom {
        get { return this.validFromField; }
        set { this.validFromField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElement( DataType = "date" )]
    public System.DateTime validTo {
        get { return this.validToField; }
        set { this.validToField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSHeaderSpurplanVersionDescription {
    private string modeField;

    private string nameField;

    private int numberField;

    private KSSHeaderSpurplanVersionDescriptionValidity validityField;

    /// <remarks/>
    public string mode {
        get { return this.modeField; }
        set { this.modeField = value; }
    }

    /// <remarks/>
    public string name {
        get { return this.nameField; }
        set { this.nameField = value; }
    }

    /// <remarks/>
    public int number {
        get { return this.numberField; }
        set { this.numberField = value; }
    }

    /// <remarks/>
    public KSSHeaderSpurplanVersionDescriptionValidity validity {
        get { return this.validityField; }
        set { this.validityField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSHeaderSpurplanVersionDescriptionValidity {
    private System.DateTime validFromField;

    private System.DateTime validToField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElement( DataType = "date" )]
    public System.DateTime validFrom {
        get { return this.validFromField; }
        set { this.validFromField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElement( DataType = "date" )]
    public System.DateTime validTo {
        get { return this.validToField; }
        set { this.validToField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSHeaderGfdiVersionDescription {
    private string modeField;

    private string nameField;

    private int numberField;

    private KSSHeaderGfdiVersionDescriptionValidity validityField;

    /// <remarks/>
    public string mode {
        get { return this.modeField; }
        set { this.modeField = value; }
    }

    /// <remarks/>
    public string name {
        get { return this.nameField; }
        set { this.nameField = value; }
    }

    /// <remarks/>
    public int number {
        get { return this.numberField; }
        set { this.numberField = value; }
    }

    /// <remarks/>
    public KSSHeaderGfdiVersionDescriptionValidity validity {
        get { return this.validityField; }
        set { this.validityField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSHeaderGfdiVersionDescriptionValidity {
    private System.DateTime validFromField;

    private System.DateTime validToField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElement( DataType = "date" )]
    public System.DateTime validFrom {
        get { return this.validFromField; }
        set { this.validFromField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElement( DataType = "date" )]
    public System.DateTime validTo {
        get { return this.validToField; }
        set { this.validToField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSEntryIndexTrassen {
    private bool isSpecialTrainField;

    private string trasseNumberField;

    private int sequenceNumberField;

    private KSSEntryIndexTrassenTimetablePeriod timetablePeriodField;

    private KSSEntryIndexTrassenValidity validityField;

    private KSSEntryIndexTrassenAdditionalTimetable additionalTimetableField;

    private System.DateTime timeOfLastChangeField;

    private int entryIndexTrassenIDField;

    private int trainIDField;

    /// <remarks/>
    public bool isSpecialTrain {
        get { return this.isSpecialTrainField; }
        set { this.isSpecialTrainField = value; }
    }

    /// <remarks/>
    public string trasseNumber {
        get { return this.trasseNumberField; }
        set { this.trasseNumberField = value; }
    }

    /// <remarks/>
    public int sequenceNumber {
        get { return this.sequenceNumberField; }
        set { this.sequenceNumberField = value; }
    }

    /// <remarks/>
    public KSSEntryIndexTrassenTimetablePeriod timetablePeriod {
        get { return this.timetablePeriodField; }
        set { this.timetablePeriodField = value; }
    }

    /// <remarks/>
    public KSSEntryIndexTrassenValidity validity {
        get { return this.validityField; }
        set { this.validityField = value; }
    }

    /// <remarks/>
    public KSSEntryIndexTrassenAdditionalTimetable additionalTimetable {
        get { return this.additionalTimetableField; }
        set { this.additionalTimetableField = value; }
    }

    /// <remarks/>
    public System.DateTime timeOfLastChange {
        get { return this.timeOfLastChangeField; }
        set { this.timeOfLastChangeField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public int entryIndexTrassenID {
        get { return this.entryIndexTrassenIDField; }
        set { this.entryIndexTrassenIDField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public int trainID {
        get { return this.trainIDField; }
        set { this.trainIDField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSEntryIndexTrassenTimetablePeriod {
    private int timetablePeriodIDField;

    private string descriptionField;

    private System.DateTime startDateField;

    private System.DateTime endDateField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public int timetablePeriodID {
        get { return this.timetablePeriodIDField; }
        set { this.timetablePeriodIDField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string description {
        get { return this.descriptionField; }
        set { this.descriptionField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute( DataType = "date" )]
    public System.DateTime startDate {
        get { return this.startDateField; }
        set { this.startDateField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute( DataType = "date" )]
    public System.DateTime endDate {
        get { return this.endDateField; }
        set { this.endDateField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSEntryIndexTrassenValidity {
    private System.DateTime validFromField;

    private System.DateTime validToField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElement( DataType = "date" )]
    public System.DateTime validFrom {
        get { return this.validFromField; }
        set { this.validFromField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElement( DataType = "date" )]
    public System.DateTime validTo {
        get { return this.validToField; }
        set { this.validToField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSEntryIndexTrassenAdditionalTimetable {
    private KSSEntryIndexTrassenAdditionalTimetableAdditionalTimetableMode additionalTimetableModeField;

    private int additionalTimetableNumberField;

    /// <remarks/>
    public KSSEntryIndexTrassenAdditionalTimetableAdditionalTimetableMode additionalTimetableMode {
        get { return this.additionalTimetableModeField; }
        set { this.additionalTimetableModeField = value; }
    }

    /// <remarks/>
    public int additionalTimetableNumber {
        get { return this.additionalTimetableNumberField; }
        set { this.additionalTimetableNumberField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSEntryIndexTrassenAdditionalTimetableAdditionalTimetableMode {
    private string typField;

    private int schluesselField;

    private string valueField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string Typ {
        get { return this.typField; }
        set { this.typField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public int Schluessel {
        get { return this.schluesselField; }
        set { this.schluesselField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlText()]
    public string Value {
        get { return this.valueField; }
        set { this.valueField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailml {
    private KSSRailmlTimetable timetableField;

    /// <remarks/>
    public KSSRailmlTimetable timetable {
        get { return this.timetableField; }
        set { this.timetableField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetable {
    private KSSRailmlTimetableTimetablePeriods timetablePeriodsField;

    private KSSRailmlTimetableTrain[] trainField;

    private decimal versionField;

    /// <remarks/>
    public KSSRailmlTimetableTimetablePeriods timetablePeriods {
        get { return this.timetablePeriodsField; }
        set { this.timetablePeriodsField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElement( "train" )]
    public KSSRailmlTimetableTrain[] train {
        get { return this.trainField; }
        set { this.trainField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public decimal version {
        get { return this.versionField; }
        set { this.versionField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTimetablePeriods {
    private KSSRailmlTimetableTimetablePeriodsTimetablePeriod timetablePeriodField;

    /// <remarks/>
    public KSSRailmlTimetableTimetablePeriodsTimetablePeriod timetablePeriod {
        get { return this.timetablePeriodField; }
        set { this.timetablePeriodField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTimetablePeriodsTimetablePeriod {
    private int timetablePeriodIDField;

    private string descriptionField;

    private System.DateTime startDateField;

    private System.DateTime endDateField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public int timetablePeriodID {
        get { return this.timetablePeriodIDField; }
        set { this.timetablePeriodIDField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string description {
        get { return this.descriptionField; }
        set { this.descriptionField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute( DataType = "date" )]
    public System.DateTime startDate {
        get { return this.startDateField; }
        set { this.startDateField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute( DataType = "date" )]
    public System.DateTime endDate {
        get { return this.endDateField; }
        set { this.endDateField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrain {
    private KSSRailmlTimetableTrainEntry[] timetableentriesField;

    private KSSRailmlTimetableTrainFineConstruction fineConstructionField;

    private int trainIDField;

    private string trainNumberField;

    private string kindField;

    private int timetablePeriodIDField;

    private string trainStatusField;

    private string remarksField;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItem( "entry", IsNullable = false )]
    public KSSRailmlTimetableTrainEntry[] timetableentries {
        get { return this.timetableentriesField; }
        set { this.timetableentriesField = value; }
    }

    /// <remarks/>
    public KSSRailmlTimetableTrainFineConstruction fineConstruction {
        get { return this.fineConstructionField; }
        set { this.fineConstructionField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public int trainID {
        get { return this.trainIDField; }
        set { this.trainIDField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string trainNumber {
        get { return this.trainNumberField; }
        set { this.trainNumberField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string kind {
        get { return this.kindField; }
        set { this.kindField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public int timetablePeriodID {
        get { return this.timetablePeriodIDField; }
        set { this.timetablePeriodIDField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string trainStatus {
        get { return this.trainStatusField; }
        set { this.trainStatusField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string remarks {
        get { return this.remarksField; }
        set { this.remarksField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainEntry {
    private KSSRailmlTimetableTrainEntryComposition compositionField;

    private KSSRailmlTimetableTrainEntrySection sectionField;

    private KSSRailmlTimetableTrainEntryAdditionalInformation additionalInformationField;

    private string posIDField;

    private string typeField;

    private System.DateTime publishedArrivalField;

    private bool publishedArrivalFieldSpecified;

    private System.DateTime publishedDepartureField;

    private bool publishedDepartureFieldSpecified;

    private string remarksField;

    private int departureDayField;

    private bool departureDayFieldSpecified;

    private int arrivalDayField;

    private bool arrivalDayFieldSpecified;

    /// <remarks/>
    public KSSRailmlTimetableTrainEntryComposition composition {
        get { return this.compositionField; }
        set { this.compositionField = value; }
    }

    /// <remarks/>
    public KSSRailmlTimetableTrainEntrySection section {
        get { return this.sectionField; }
        set { this.sectionField = value; }
    }

    /// <remarks/>
    public KSSRailmlTimetableTrainEntryAdditionalInformation additionalInformation {
        get { return this.additionalInformationField; }
        set { this.additionalInformationField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string posID {
        get { return this.posIDField; }
        set { this.posIDField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string type {
        get { return this.typeField; }
        set { this.typeField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute( DataType = "time" )]
    public System.DateTime publishedArrival {
        get { return this.publishedArrivalField; }
        set { this.publishedArrivalField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnore()]
    public bool publishedArrivalSpecified {
        get { return this.publishedArrivalFieldSpecified; }
        set { this.publishedArrivalFieldSpecified = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute( DataType = "time" )]
    public System.DateTime publishedDeparture {
        get { return this.publishedDepartureField; }
        set { this.publishedDepartureField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnore()]
    public bool publishedDepartureSpecified {
        get { return this.publishedDepartureFieldSpecified; }
        set { this.publishedDepartureFieldSpecified = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string remarks {
        get { return this.remarksField; }
        set { this.remarksField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public int departureDay {
        get { return this.departureDayField; }
        set { this.departureDayField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnore()]
    public bool departureDaySpecified {
        get { return this.departureDayFieldSpecified; }
        set { this.departureDayFieldSpecified = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public int arrivalDay {
        get { return this.arrivalDayField; }
        set { this.arrivalDayField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnore()]
    public bool arrivalDaySpecified {
        get { return this.arrivalDayFieldSpecified; }
        set { this.arrivalDayFieldSpecified = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainEntryComposition {
    private KSSRailmlTimetableTrainEntryCompositionService serviceField;

    private KSSRailmlTimetableTrainEntryCompositionDynamic dynamicField;

    private int compIDField;

    /// <remarks/>
    public KSSRailmlTimetableTrainEntryCompositionService service {
        get { return this.serviceField; }
        set { this.serviceField = value; }
    }

    /// <remarks/>
    public KSSRailmlTimetableTrainEntryCompositionDynamic dynamic {
        get { return this.dynamicField; }
        set { this.dynamicField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public int compID {
        get { return this.compIDField; }
        set { this.compIDField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainEntryCompositionService {
    private string bitMaskField;

    private System.DateTime startDateField;

    private System.DateTime endDateField;

    private string descriptionField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute( DataType = "integer" )]
    public string bitMask {
        get { return this.bitMaskField; }
        set { this.bitMaskField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute( DataType = "date" )]
    public System.DateTime startDate {
        get { return this.startDateField; }
        set { this.startDateField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute( DataType = "date" )]
    public System.DateTime endDate {
        get { return this.endDateField; }
        set { this.endDateField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string description {
        get { return this.descriptionField; }
        set { this.descriptionField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainEntryCompositionDynamic {
    private string trainProtectionField;

    private int brakedWeightPercentageField;

    private string brakingSystemField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string trainProtection {
        get { return this.trainProtectionField; }
        set { this.trainProtectionField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public int brakedWeightPercentage {
        get { return this.brakedWeightPercentageField; }
        set { this.brakedWeightPercentageField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string brakingSystem {
        get { return this.brakingSystemField; }
        set { this.brakingSystemField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainEntrySection {
    private string trackIDField;

    private int sectionIDField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string trackID {
        get { return this.trackIDField; }
        set { this.trackIDField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public int sectionID {
        get { return this.sectionIDField; }
        set { this.sectionIDField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainEntryAdditionalInformation {
    private object[] itemsField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElement( "basicCharacteristic", typeof( KSSRailmlTimetableTrainEntryAdditionalInformationBasicCharacteristic ) )]
    [System.Xml.Serialization.XmlElement( "operationalArrangementNotes", typeof( KSSRailmlTimetableTrainEntryAdditionalInformationOperationalArrangementNotes ) )]
    [System.Xml.Serialization.XmlElement( "operationalNotes", typeof( KSSRailmlTimetableTrainEntryAdditionalInformationOperationalNotes ) )]
    [System.Xml.Serialization.XmlElement( "scheduleTimeNotes", typeof( KSSRailmlTimetableTrainEntryAdditionalInformationScheduleTimeNotes ) )]
    public object[] Items {
        get { return this.itemsField; }
        set { this.itemsField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainEntryAdditionalInformationBasicCharacteristic {
    private KSSRailmlTimetableTrainEntryAdditionalInformationBasicCharacteristicService serviceField;

    private bool onDemandField;

    private KSSRailmlTimetableTrainEntryAdditionalInformationBasicCharacteristicTractionUnit[] tractionUnitsField;

    private int numberOfPassengerCarsField;

    private decimal trainsetLengthField;

    private decimal trainsetWeightField;

    private bool tonnageRatingField;

    private int trainsetVelocityField;

    private int constructionVelocityField;

    private bool constructionVelocityFieldSpecified;

    private int maxVelocityField;

    private KSSRailmlTimetableTrainEntryAdditionalInformationBasicCharacteristicBodyTiltingTechnique bodyTiltingTechniqueField;

    private bool lockingTripField;

    private KSSRailmlTimetableTrainEntryAdditionalInformationBasicCharacteristicCarriageSpecifics carriageSpecificsField;

    /// <remarks/>
    public KSSRailmlTimetableTrainEntryAdditionalInformationBasicCharacteristicService service {
        get { return this.serviceField; }
        set { this.serviceField = value; }
    }

    /// <remarks/>
    public bool onDemand {
        get { return this.onDemandField; }
        set { this.onDemandField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItem( "tractionUnit", IsNullable = false )]
    public KSSRailmlTimetableTrainEntryAdditionalInformationBasicCharacteristicTractionUnit[] tractionUnits {
        get { return this.tractionUnitsField; }
        set { this.tractionUnitsField = value; }
    }

    /// <remarks/>
    public int numberOfPassengerCars {
        get { return this.numberOfPassengerCarsField; }
        set { this.numberOfPassengerCarsField = value; }
    }

    /// <remarks/>
    public decimal trainsetLength {
        get { return this.trainsetLengthField; }
        set { this.trainsetLengthField = value; }
    }

    /// <remarks/>
    public decimal trainsetWeight {
        get { return this.trainsetWeightField; }
        set { this.trainsetWeightField = value; }
    }

    /// <remarks/>
    public bool tonnageRating {
        get { return this.tonnageRatingField; }
        set { this.tonnageRatingField = value; }
    }

    /// <remarks/>
    public int trainsetVelocity {
        get { return this.trainsetVelocityField; }
        set { this.trainsetVelocityField = value; }
    }

    /// <remarks/>
    public int constructionVelocity {
        get { return this.constructionVelocityField; }
        set { this.constructionVelocityField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnore()]
    public bool constructionVelocitySpecified {
        get { return this.constructionVelocityFieldSpecified; }
        set { this.constructionVelocityFieldSpecified = value; }
    }

    /// <remarks/>
    public int maxVelocity {
        get { return this.maxVelocityField; }
        set { this.maxVelocityField = value; }
    }

    /// <remarks/>
    public KSSRailmlTimetableTrainEntryAdditionalInformationBasicCharacteristicBodyTiltingTechnique bodyTiltingTechnique {
        get { return this.bodyTiltingTechniqueField; }
        set { this.bodyTiltingTechniqueField = value; }
    }

    /// <remarks/>
    public bool lockingTrip {
        get { return this.lockingTripField; }
        set { this.lockingTripField = value; }
    }

    /// <remarks/>
    public KSSRailmlTimetableTrainEntryAdditionalInformationBasicCharacteristicCarriageSpecifics carriageSpecifics {
        get { return this.carriageSpecificsField; }
        set { this.carriageSpecificsField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainEntryAdditionalInformationBasicCharacteristicService {
    private string bitMaskField;

    private System.DateTime startDateField;

    private System.DateTime endDateField;

    private string descriptionField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute( DataType = "integer" )]
    public string bitMask {
        get { return this.bitMaskField; }
        set { this.bitMaskField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute( DataType = "date" )]
    public System.DateTime startDate {
        get { return this.startDateField; }
        set { this.startDateField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute( DataType = "date" )]
    public System.DateTime endDate {
        get { return this.endDateField; }
        set { this.endDateField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string description {
        get { return this.descriptionField; }
        set { this.descriptionField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainEntryAdditionalInformationBasicCharacteristicTractionUnit {
    private bool isStandardTractionUnitField;

    private KSSRailmlTimetableTrainEntryAdditionalInformationBasicCharacteristicTractionUnitPosition positionField;

    private KSSRailmlTimetableTrainEntryAdditionalInformationBasicCharacteristicTractionUnitTractionUnitDesignSeries tractionUnitDesignSeriesField;

    /// <remarks/>
    public bool isStandardTractionUnit {
        get { return this.isStandardTractionUnitField; }
        set { this.isStandardTractionUnitField = value; }
    }

    /// <remarks/>
    public KSSRailmlTimetableTrainEntryAdditionalInformationBasicCharacteristicTractionUnitPosition position {
        get { return this.positionField; }
        set { this.positionField = value; }
    }

    /// <remarks/>
    public KSSRailmlTimetableTrainEntryAdditionalInformationBasicCharacteristicTractionUnitTractionUnitDesignSeries tractionUnitDesignSeries {
        get { return this.tractionUnitDesignSeriesField; }
        set { this.tractionUnitDesignSeriesField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainEntryAdditionalInformationBasicCharacteristicTractionUnitPosition {
    private string typField;

    private int schluesselField;

    private string valueField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string Typ {
        get { return this.typField; }
        set { this.typField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public int Schluessel {
        get { return this.schluesselField; }
        set { this.schluesselField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlText()]
    public string Value {
        get { return this.valueField; }
        set { this.valueField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainEntryAdditionalInformationBasicCharacteristicTractionUnitTractionUnitDesignSeries {
    private KSSRailmlTimetableTrainEntryAdditionalInformationBasicCharacteristicTractionUnitTractionUnitDesignSeriesDesignSeries designSeriesField;

    private byte varianteField;

    private KSSRailmlTimetableTrainEntryAdditionalInformationBasicCharacteristicTractionUnitTractionUnitDesignSeriesStromart stromartField;

    /// <remarks/>
    public KSSRailmlTimetableTrainEntryAdditionalInformationBasicCharacteristicTractionUnitTractionUnitDesignSeriesDesignSeries designSeries {
        get { return this.designSeriesField; }
        set { this.designSeriesField = value; }
    }

    /// <remarks/>
    public byte variante {
        get { return this.varianteField; }
        set { this.varianteField = value; }
    }

    /// <remarks/>
    public KSSRailmlTimetableTrainEntryAdditionalInformationBasicCharacteristicTractionUnitTractionUnitDesignSeriesStromart stromart {
        get { return this.stromartField; }
        set { this.stromartField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainEntryAdditionalInformationBasicCharacteristicTractionUnitTractionUnitDesignSeriesDesignSeries {
    private string typField;

    private ushort nrField;

    private string valueField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string Typ {
        get { return this.typField; }
        set { this.typField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public ushort Nr {
        get { return this.nrField; }
        set { this.nrField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlText()]
    public string Value {
        get { return this.valueField; }
        set { this.valueField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainEntryAdditionalInformationBasicCharacteristicTractionUnitTractionUnitDesignSeriesStromart {
    private string typField;

    private byte schluesselField;

    private string valueField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string Typ {
        get { return this.typField; }
        set { this.typField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public byte Schluessel {
        get { return this.schluesselField; }
        set { this.schluesselField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlText()]
    public string Value {
        get { return this.valueField; }
        set { this.valueField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainEntryAdditionalInformationBasicCharacteristicBodyTiltingTechnique {
    private string typField;

    private int schluesselField;

    private string valueField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string Typ {
        get { return this.typField; }
        set { this.typField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public int Schluessel {
        get { return this.schluesselField; }
        set { this.schluesselField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlText()]
    public string Value {
        get { return this.valueField; }
        set { this.valueField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainEntryAdditionalInformationBasicCharacteristicCarriageSpecifics {
    private KSSRailmlTimetableTrainEntryAdditionalInformationBasicCharacteristicCarriageSpecificsCarriageSpecific carriageSpecificField;

    /// <remarks/>
    public KSSRailmlTimetableTrainEntryAdditionalInformationBasicCharacteristicCarriageSpecificsCarriageSpecific carriageSpecific {
        get { return this.carriageSpecificField; }
        set { this.carriageSpecificField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainEntryAdditionalInformationBasicCharacteristicCarriageSpecificsCarriageSpecific {
    private KSSRailmlTimetableTrainEntryAdditionalInformationBasicCharacteristicCarriageSpecificsCarriageSpecificTemplateTitle templateTitleField;

    private string textField;

    /// <remarks/>
    public KSSRailmlTimetableTrainEntryAdditionalInformationBasicCharacteristicCarriageSpecificsCarriageSpecificTemplateTitle templateTitle {
        get { return this.templateTitleField; }
        set { this.templateTitleField = value; }
    }

    /// <remarks/>
    public string text {
        get { return this.textField; }
        set { this.textField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainEntryAdditionalInformationBasicCharacteristicCarriageSpecificsCarriageSpecificTemplateTitle {
    private string kategorieField;

    private string schluesselField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string Kategorie {
        get { return this.kategorieField; }
        set { this.kategorieField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string Schluessel {
        get { return this.schluesselField; }
        set { this.schluesselField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainEntryAdditionalInformationOperationalArrangementNotes {
    private KSSRailmlTimetableTrainEntryAdditionalInformationOperationalArrangementNotesArrangementNote[] arrangementNoteField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElement( "arrangementNote" )]
    public KSSRailmlTimetableTrainEntryAdditionalInformationOperationalArrangementNotesArrangementNote[] arrangementNote {
        get { return this.arrangementNoteField; }
        set { this.arrangementNoteField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainEntryAdditionalInformationOperationalArrangementNotesArrangementNote {
    private string servicePointField;

    private KSSRailmlTimetableTrainEntryAdditionalInformationOperationalArrangementNotesArrangementNoteTemplateTitle templateTitleField;

    private string textField;

    private KSSRailmlTimetableTrainEntryAdditionalInformationOperationalArrangementNotesArrangementNoteService serviceField;

    private string toServicePointField;

    /// <remarks/>
    public string servicePoint {
        get { return this.servicePointField; }
        set { this.servicePointField = value; }
    }

    /// <remarks/>
    public KSSRailmlTimetableTrainEntryAdditionalInformationOperationalArrangementNotesArrangementNoteTemplateTitle templateTitle {
        get { return this.templateTitleField; }
        set { this.templateTitleField = value; }
    }

    /// <remarks/>
    public string text {
        get { return this.textField; }
        set { this.textField = value; }
    }

    /// <remarks/>
    public KSSRailmlTimetableTrainEntryAdditionalInformationOperationalArrangementNotesArrangementNoteService service {
        get { return this.serviceField; }
        set { this.serviceField = value; }
    }

    /// <remarks/>
    public string toServicePoint {
        get { return this.toServicePointField; }
        set { this.toServicePointField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainEntryAdditionalInformationOperationalArrangementNotesArrangementNoteTemplateTitle {
    private string kategorieField;

    private string schluesselField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string Kategorie {
        get { return this.kategorieField; }
        set { this.kategorieField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string Schluessel {
        get { return this.schluesselField; }
        set { this.schluesselField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainEntryAdditionalInformationOperationalArrangementNotesArrangementNoteService {
    private string bitMaskField;

    private System.DateTime startDateField;

    private System.DateTime endDateField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string bitMask {
        get { return this.bitMaskField; }
        set { this.bitMaskField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute( DataType = "date" )]
    public System.DateTime startDate {
        get { return this.startDateField; }
        set { this.startDateField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute( DataType = "date" )]
    public System.DateTime endDate {
        get { return this.endDateField; }
        set { this.endDateField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainEntryAdditionalInformationOperationalNotes {
    private KSSRailmlTimetableTrainEntryAdditionalInformationOperationalNotesNote[] noteField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElement( "note" )]
    public KSSRailmlTimetableTrainEntryAdditionalInformationOperationalNotesNote[] note {
        get { return this.noteField; }
        set { this.noteField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainEntryAdditionalInformationOperationalNotesNote {
    private string servicePointField;

    private KSSRailmlTimetableTrainEntryAdditionalInformationOperationalNotesNoteTemplateTitle templateTitleField;

    private string textField;

    private KSSRailmlTimetableTrainEntryAdditionalInformationOperationalNotesNoteService serviceField;

    /// <remarks/>
    public string servicePoint {
        get { return this.servicePointField; }
        set { this.servicePointField = value; }
    }

    /// <remarks/>
    public KSSRailmlTimetableTrainEntryAdditionalInformationOperationalNotesNoteTemplateTitle templateTitle {
        get { return this.templateTitleField; }
        set { this.templateTitleField = value; }
    }

    /// <remarks/>
    public string text {
        get { return this.textField; }
        set { this.textField = value; }
    }

    /// <remarks/>
    public KSSRailmlTimetableTrainEntryAdditionalInformationOperationalNotesNoteService service {
        get { return this.serviceField; }
        set { this.serviceField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainEntryAdditionalInformationOperationalNotesNoteTemplateTitle {
    private int kategorieField;

    private string schluesselField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public int Kategorie {
        get { return this.kategorieField; }
        set { this.kategorieField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string Schluessel {
        get { return this.schluesselField; }
        set { this.schluesselField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainEntryAdditionalInformationOperationalNotesNoteService {
    private string bitMaskField;

    private System.DateTime startDateField;

    private System.DateTime endDateField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute( DataType = "integer" )]
    public string bitMask {
        get { return this.bitMaskField; }
        set { this.bitMaskField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute( DataType = "date" )]
    public System.DateTime startDate {
        get { return this.startDateField; }
        set { this.startDateField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute( DataType = "date" )]
    public System.DateTime endDate {
        get { return this.endDateField; }
        set { this.endDateField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainEntryAdditionalInformationScheduleTimeNotes {
    private KSSRailmlTimetableTrainEntryAdditionalInformationScheduleTimeNotesNote noteField;

    /// <remarks/>
    public KSSRailmlTimetableTrainEntryAdditionalInformationScheduleTimeNotesNote note {
        get { return this.noteField; }
        set { this.noteField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainEntryAdditionalInformationScheduleTimeNotesNote {
    private string servicePointField;

    private KSSRailmlTimetableTrainEntryAdditionalInformationScheduleTimeNotesNoteTemplateTitle templateTitleField;

    private string textField;

    /// <remarks/>
    public string servicePoint {
        get { return this.servicePointField; }
        set { this.servicePointField = value; }
    }

    /// <remarks/>
    public KSSRailmlTimetableTrainEntryAdditionalInformationScheduleTimeNotesNoteTemplateTitle templateTitle {
        get { return this.templateTitleField; }
        set { this.templateTitleField = value; }
    }

    /// <remarks/>
    public string text {
        get { return this.textField; }
        set { this.textField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainEntryAdditionalInformationScheduleTimeNotesNoteTemplateTitle {
    private string kategorieField;

    private string schluesselField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string Kategorie {
        get { return this.kategorieField; }
        set { this.kategorieField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string Schluessel {
        get { return this.schluesselField; }
        set { this.schluesselField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainFineConstruction {
    private KSSRailmlTimetableTrainFineConstructionConstructionTrain[] constructionTrainField;

    private KSSRailmlTimetableTrainFineConstructionConnection[] connectionField;

    private int entryIndexTrassenIDField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElement( "constructionTrain" )]
    public KSSRailmlTimetableTrainFineConstructionConstructionTrain[] constructionTrain {
        get { return this.constructionTrainField; }
        set { this.constructionTrainField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElement( "connection" )]
    public KSSRailmlTimetableTrainFineConstructionConnection[] connection {
        get { return this.connectionField; }
        set { this.connectionField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public int entryIndexTrassenID {
        get { return this.entryIndexTrassenIDField; }
        set { this.entryIndexTrassenIDField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainFineConstructionConstructionTrain {
    private KSSRailmlTimetableTrainFineConstructionConstructionTrainTrainNumber trainNumberField;

    private string statusField;

    private KSSRailmlTimetableTrainFineConstructionConstructionTrainCharacteristic characteristicField;

    private KSSRailmlTimetableTrainFineConstructionConstructionTrainService[] servicesField;

    private KSSRailmlTimetableTrainFineConstructionConstructionTrainSequence sequenceField;

    /// <remarks/>
    public KSSRailmlTimetableTrainFineConstructionConstructionTrainTrainNumber trainNumber {
        get { return this.trainNumberField; }
        set { this.trainNumberField = value; }
    }

    /// <remarks/>
    public string status {
        get { return this.statusField; }
        set { this.statusField = value; }
    }

    /// <remarks/>
    public KSSRailmlTimetableTrainFineConstructionConstructionTrainCharacteristic characteristic {
        get { return this.characteristicField; }
        set { this.characteristicField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItem( "service", IsNullable = false )]
    public KSSRailmlTimetableTrainFineConstructionConstructionTrainService[] services {
        get { return this.servicesField; }
        set { this.servicesField = value; }
    }

    /// <remarks/>
    public KSSRailmlTimetableTrainFineConstructionConstructionTrainSequence sequence {
        get { return this.sequenceField; }
        set { this.sequenceField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainFineConstructionConstructionTrainTrainNumber {
    private int mainNumberField;

    private int subNumberField;

    private string userAbbreviationField;

    /// <remarks/>
    public int mainNumber {
        get { return this.mainNumberField; }
        set { this.mainNumberField = value; }
    }

    /// <remarks/>
    public int subNumber {
        get { return this.subNumberField; }
        set { this.subNumberField = value; }
    }

    /// <remarks/>
    public string userAbbreviation {
        get { return this.userAbbreviationField; }
        set { this.userAbbreviationField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainFineConstructionConstructionTrainCharacteristic {
    private bool onDemandField;

    private KSSRailmlTimetableTrainFineConstructionConstructionTrainCharacteristicTractionUnit[] tractionUnitsField;

    private int numberOfPassengerCarsField;

    private decimal trainsetLengthField;

    private decimal trainsetWeightField;

    private bool tonnageRatingField;

    private int trainsetVelocityField;

    private int constructionVelocityField;

    private bool constructionVelocityFieldSpecified;

    private int maxVelocityField;

    private KSSRailmlTimetableTrainFineConstructionConstructionTrainCharacteristicBodyTiltingTechnique bodyTiltingTechniqueField;

    private bool lockingTripField;

    private KSSRailmlTimetableTrainFineConstructionConstructionTrainCharacteristicCarriageSpecifics carriageSpecificsField;

    private bool isPhantomField;

    private string commentField;

    private KSSRailmlTimetableTrainFineConstructionConstructionTrainCharacteristicKind kindField;

    private string trainClassField;

    private string relevantStopPositionModeField;

    private bool trainProtectionField;

    private decimal totalLengthField;

    private decimal totalWeightField;

    private int brakedWeightPercentageField;

    private KSSRailmlTimetableTrainFineConstructionConstructionTrainCharacteristicBrakingSystem brakingSystemField;

    private int recoveryMarginProportionField;

    /// <remarks/>
    public bool onDemand {
        get { return this.onDemandField; }
        set { this.onDemandField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItem( "tractionUnit", IsNullable = false )]
    public KSSRailmlTimetableTrainFineConstructionConstructionTrainCharacteristicTractionUnit[] tractionUnits {
        get { return this.tractionUnitsField; }
        set { this.tractionUnitsField = value; }
    }

    /// <remarks/>
    public int numberOfPassengerCars {
        get { return this.numberOfPassengerCarsField; }
        set { this.numberOfPassengerCarsField = value; }
    }

    /// <remarks/>
    public decimal trainsetLength {
        get { return this.trainsetLengthField; }
        set { this.trainsetLengthField = value; }
    }

    /// <remarks/>
    public decimal trainsetWeight {
        get { return this.trainsetWeightField; }
        set { this.trainsetWeightField = value; }
    }

    /// <remarks/>
    public bool tonnageRating {
        get { return this.tonnageRatingField; }
        set { this.tonnageRatingField = value; }
    }

    /// <remarks/>
    public int trainsetVelocity {
        get { return this.trainsetVelocityField; }
        set { this.trainsetVelocityField = value; }
    }

    /// <remarks/>
    public int constructionVelocity {
        get { return this.constructionVelocityField; }
        set { this.constructionVelocityField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnore()]
    public bool constructionVelocitySpecified {
        get { return this.constructionVelocityFieldSpecified; }
        set { this.constructionVelocityFieldSpecified = value; }
    }

    /// <remarks/>
    public int maxVelocity {
        get { return this.maxVelocityField; }
        set { this.maxVelocityField = value; }
    }

    /// <remarks/>
    public KSSRailmlTimetableTrainFineConstructionConstructionTrainCharacteristicBodyTiltingTechnique bodyTiltingTechnique {
        get { return this.bodyTiltingTechniqueField; }
        set { this.bodyTiltingTechniqueField = value; }
    }

    /// <remarks/>
    public bool lockingTrip {
        get { return this.lockingTripField; }
        set { this.lockingTripField = value; }
    }

    /// <remarks/>
    public KSSRailmlTimetableTrainFineConstructionConstructionTrainCharacteristicCarriageSpecifics carriageSpecifics {
        get { return this.carriageSpecificsField; }
        set { this.carriageSpecificsField = value; }
    }

    /// <remarks/>
    public bool isPhantom {
        get { return this.isPhantomField; }
        set { this.isPhantomField = value; }
    }

    /// <remarks/>
    public string comment {
        get { return this.commentField; }
        set { this.commentField = value; }
    }

    /// <remarks/>
    public KSSRailmlTimetableTrainFineConstructionConstructionTrainCharacteristicKind kind {
        get { return this.kindField; }
        set { this.kindField = value; }
    }

    /// <remarks/>
    public string trainClass {
        get { return this.trainClassField; }
        set { this.trainClassField = value; }
    }

    /// <remarks/>
    public string relevantStopPositionMode {
        get { return this.relevantStopPositionModeField; }
        set { this.relevantStopPositionModeField = value; }
    }

    /// <remarks/>
    public bool trainProtection {
        get { return this.trainProtectionField; }
        set { this.trainProtectionField = value; }
    }

    /// <remarks/>
    public decimal totalLength {
        get { return this.totalLengthField; }
        set { this.totalLengthField = value; }
    }

    /// <remarks/>
    public decimal totalWeight {
        get { return this.totalWeightField; }
        set { this.totalWeightField = value; }
    }

    /// <remarks/>
    public int brakedWeightPercentage {
        get { return this.brakedWeightPercentageField; }
        set { this.brakedWeightPercentageField = value; }
    }

    /// <remarks/>
    public KSSRailmlTimetableTrainFineConstructionConstructionTrainCharacteristicBrakingSystem brakingSystem {
        get { return this.brakingSystemField; }
        set { this.brakingSystemField = value; }
    }

    /// <remarks/>
    public int recoveryMarginProportion {
        get { return this.recoveryMarginProportionField; }
        set { this.recoveryMarginProportionField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainFineConstructionConstructionTrainCharacteristicTractionUnit {
    private bool isStandardTractionUnitField;

    private KSSRailmlTimetableTrainFineConstructionConstructionTrainCharacteristicTractionUnitPosition positionField;

    private KSSRailmlTimetableTrainFineConstructionConstructionTrainCharacteristicTractionUnitTractionUnitDesignSeries tractionUnitDesignSeriesField;

    /// <remarks/>
    public bool isStandardTractionUnit {
        get { return this.isStandardTractionUnitField; }
        set { this.isStandardTractionUnitField = value; }
    }

    /// <remarks/>
    public KSSRailmlTimetableTrainFineConstructionConstructionTrainCharacteristicTractionUnitPosition position {
        get { return this.positionField; }
        set { this.positionField = value; }
    }

    /// <remarks/>
    public KSSRailmlTimetableTrainFineConstructionConstructionTrainCharacteristicTractionUnitTractionUnitDesignSeries tractionUnitDesignSeries {
        get { return this.tractionUnitDesignSeriesField; }
        set { this.tractionUnitDesignSeriesField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainFineConstructionConstructionTrainCharacteristicTractionUnitPosition {
    private string typField;

    private int schluesselField;

    private string valueField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string Typ {
        get { return this.typField; }
        set { this.typField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public int Schluessel {
        get { return this.schluesselField; }
        set { this.schluesselField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlText()]
    public string Value {
        get { return this.valueField; }
        set { this.valueField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainFineConstructionConstructionTrainCharacteristicTractionUnitTractionUnitDesignSeries {
    private KSSRailmlTimetableTrainFineConstructionConstructionTrainCharacteristicTractionUnitTractionUnitDesignSeriesDesignSeries designSeriesField;

    private int varianteField;

    private KSSRailmlTimetableTrainFineConstructionConstructionTrainCharacteristicTractionUnitTractionUnitDesignSeriesStromart stromartField;

    /// <remarks/>
    public KSSRailmlTimetableTrainFineConstructionConstructionTrainCharacteristicTractionUnitTractionUnitDesignSeriesDesignSeries designSeries {
        get { return this.designSeriesField; }
        set { this.designSeriesField = value; }
    }

    /// <remarks/>
    public int variante {
        get { return this.varianteField; }
        set { this.varianteField = value; }
    }

    /// <remarks/>
    public KSSRailmlTimetableTrainFineConstructionConstructionTrainCharacteristicTractionUnitTractionUnitDesignSeriesStromart stromart {
        get { return this.stromartField; }
        set { this.stromartField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainFineConstructionConstructionTrainCharacteristicTractionUnitTractionUnitDesignSeriesDesignSeries {
    private string typField;
    private int nrField;
    private string valueField;

    [System.Xml.Serialization.XmlAttribute()]
    public string Typ {
        get { return this.typField; }
        set { this.typField = value; }
    }

    [System.Xml.Serialization.XmlAttribute()]
    public int Nr {
        get { return this.nrField; }
        set { this.nrField = value; }
    }

    [System.Xml.Serialization.XmlText()]
    public string Value {
        get { return this.valueField; }
        set { this.valueField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainFineConstructionConstructionTrainCharacteristicTractionUnitTractionUnitDesignSeriesStromart {
    private string typField;

    private int schluesselField;

    private string valueField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string Typ {
        get { return this.typField; }
        set { this.typField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public int Schluessel {
        get { return this.schluesselField; }
        set { this.schluesselField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlText()]
    public string Value {
        get { return this.valueField; }
        set { this.valueField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainFineConstructionConstructionTrainCharacteristicBodyTiltingTechnique {
    private string typField;

    private int schluesselField;

    private string valueField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string Typ {
        get { return this.typField; }
        set { this.typField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public int Schluessel {
        get { return this.schluesselField; }
        set { this.schluesselField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlText()]
    public string Value {
        get { return this.valueField; }
        set { this.valueField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainFineConstructionConstructionTrainCharacteristicCarriageSpecifics {
    private KSSRailmlTimetableTrainFineConstructionConstructionTrainCharacteristicCarriageSpecificsCarriageSpecific carriageSpecificField;

    /// <remarks/>
    public KSSRailmlTimetableTrainFineConstructionConstructionTrainCharacteristicCarriageSpecificsCarriageSpecific carriageSpecific {
        get { return this.carriageSpecificField; }
        set { this.carriageSpecificField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainFineConstructionConstructionTrainCharacteristicCarriageSpecificsCarriageSpecific {
    private KSSRailmlTimetableTrainFineConstructionConstructionTrainCharacteristicCarriageSpecificsCarriageSpecificTemplateTitle templateTitleField;

    private string textField;

    /// <remarks/>
    public KSSRailmlTimetableTrainFineConstructionConstructionTrainCharacteristicCarriageSpecificsCarriageSpecificTemplateTitle templateTitle {
        get { return this.templateTitleField; }
        set { this.templateTitleField = value; }
    }

    /// <remarks/>
    public string text {
        get { return this.textField; }
        set { this.textField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainFineConstructionConstructionTrainCharacteristicCarriageSpecificsCarriageSpecificTemplateTitle {
    private string kategorieField;

    private string schluesselField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string Kategorie {
        get { return this.kategorieField; }
        set { this.kategorieField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string Schluessel {
        get { return this.schluesselField; }
        set { this.schluesselField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainFineConstructionConstructionTrainCharacteristicKind {
    private int hauptnummerField;

    private string zuggattungsProduktField;

    private int valueField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public int Hauptnummer {
        get { return this.hauptnummerField; }
        set { this.hauptnummerField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string ZuggattungsProdukt {
        get { return this.zuggattungsProduktField; }
        set { this.zuggattungsProduktField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlText()]
    public int Value {
        get { return this.valueField; }
        set { this.valueField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainFineConstructionConstructionTrainCharacteristicBrakingSystem {
    private string typField;

    private int schluesselField;

    private string valueField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string Typ {
        get { return this.typField; }
        set { this.typField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public int Schluessel {
        get { return this.schluesselField; }
        set { this.schluesselField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlText()]
    public string Value {
        get { return this.valueField; }
        set { this.valueField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainFineConstructionConstructionTrainService {
    private KSSRailmlTimetableTrainFineConstructionConstructionTrainServiceOperatingDay[] operatingDayField;

    private KSSRailmlTimetableTrainFineConstructionConstructionTrainServiceSpecial[] specialField;

    private string descriptionField;

    private System.DateTime startDateField;

    private bool startDateFieldSpecified;

    private System.DateTime endDateField;

    private bool endDateFieldSpecified;

    private string bitMaskField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElement( "operatingDay" )]
    public KSSRailmlTimetableTrainFineConstructionConstructionTrainServiceOperatingDay[] operatingDay {
        get { return this.operatingDayField; }
        set { this.operatingDayField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElement( "special" )]
    public KSSRailmlTimetableTrainFineConstructionConstructionTrainServiceSpecial[] special {
        get { return this.specialField; }
        set { this.specialField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string description {
        get { return this.descriptionField; }
        set { this.descriptionField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute( DataType = "date" )]
    public System.DateTime startDate {
        get { return this.startDateField; }
        set { this.startDateField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnore()]
    public bool startDateSpecified {
        get { return this.startDateFieldSpecified; }
        set { this.startDateFieldSpecified = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute( DataType = "date" )]
    public System.DateTime endDate {
        get { return this.endDateField; }
        set { this.endDateField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnore()]
    public bool endDateSpecified {
        get { return this.endDateFieldSpecified; }
        set { this.endDateFieldSpecified = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute( DataType = "integer" )]
    public string bitMask {
        get { return this.bitMaskField; }
        set { this.bitMaskField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainFineConstructionConstructionTrainServiceOperatingDay {
    private int operatingCodeField;

    private string dayTypeField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public int operatingCode {
        get { return this.operatingCodeField; }
        set { this.operatingCodeField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string dayType {
        get { return this.dayTypeField; }
        set { this.dayTypeField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainFineConstructionConstructionTrainServiceSpecial {
    private string typeField;

    private System.DateTime dateField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string type {
        get { return this.typeField; }
        set { this.typeField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute( DataType = "date" )]
    public System.DateTime date {
        get { return this.dateField; }
        set { this.dateField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainFineConstructionConstructionTrainSequence {
    private bool breakinField;

    private bool breakoutField;

    private KSSRailmlTimetableTrainFineConstructionConstructionTrainSequenceSequenceServicePoint[] sequenceServicePointsField;

    private KSSRailmlTimetableTrainFineConstructionConstructionTrainSequenceAnchor anchorField;

    /// <remarks/>
    public bool breakin {
        get { return this.breakinField; }
        set { this.breakinField = value; }
    }

    /// <remarks/>
    public bool breakout {
        get { return this.breakoutField; }
        set { this.breakoutField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItem( "sequenceServicePoint", IsNullable = false )]
    public KSSRailmlTimetableTrainFineConstructionConstructionTrainSequenceSequenceServicePoint[] sequenceServicePoints {
        get { return this.sequenceServicePointsField; }
        set { this.sequenceServicePointsField = value; }
    }

    /// <remarks/>
    public KSSRailmlTimetableTrainFineConstructionConstructionTrainSequenceAnchor anchor {
        get { return this.anchorField; }
        set { this.anchorField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainFineConstructionConstructionTrainSequenceSequenceServicePoint {
    private string servicePointField;

    private string longAppellationField;

    private string trackSystemField;

    private KSSRailmlTimetableTrainFineConstructionConstructionTrainSequenceSequenceServicePointArrival arrivalField;

    private KSSRailmlTimetableTrainFineConstructionConstructionTrainSequenceSequenceServicePointPublishedArrival publishedArrivalField;

    private KSSRailmlTimetableTrainFineConstructionConstructionTrainSequenceSequenceServicePointDeparture departureField;

    private KSSRailmlTimetableTrainFineConstructionConstructionTrainSequenceSequenceServicePointPublishedDeparture publishedDepartureField;

    private KSSRailmlTimetableTrainFineConstructionConstructionTrainSequenceSequenceServicePointStopMode stopModeField;

    private string minStopTimeField;

    /// <remarks/>
    public string servicePoint {
        get { return this.servicePointField; }
        set { this.servicePointField = value; }
    }

    /// <remarks/>
    public string longAppellation {
        get { return this.longAppellationField; }
        set { this.longAppellationField = value; }
    }

    /// <remarks/>
    public string trackSystem {
        get { return this.trackSystemField; }
        set { this.trackSystemField = value; }
    }

    /// <remarks/>
    public KSSRailmlTimetableTrainFineConstructionConstructionTrainSequenceSequenceServicePointArrival arrival {
        get { return this.arrivalField; }
        set { this.arrivalField = value; }
    }

    /// <remarks/>
    public KSSRailmlTimetableTrainFineConstructionConstructionTrainSequenceSequenceServicePointPublishedArrival publishedArrival {
        get { return this.publishedArrivalField; }
        set { this.publishedArrivalField = value; }
    }

    /// <remarks/>
    public KSSRailmlTimetableTrainFineConstructionConstructionTrainSequenceSequenceServicePointDeparture departure {
        get { return this.departureField; }
        set { this.departureField = value; }
    }

    /// <remarks/>
    public KSSRailmlTimetableTrainFineConstructionConstructionTrainSequenceSequenceServicePointPublishedDeparture publishedDeparture {
        get { return this.publishedDepartureField; }
        set { this.publishedDepartureField = value; }
    }

    /// <remarks/>
    public KSSRailmlTimetableTrainFineConstructionConstructionTrainSequenceSequenceServicePointStopMode stopMode {
        get { return this.stopModeField; }
        set { this.stopModeField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElement( DataType = "duration" )]
    public string minStopTime {
        get { return this.minStopTimeField; }
        set { this.minStopTimeField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainFineConstructionConstructionTrainSequenceSequenceServicePointArrival {
    private int nightLeapField;

    private bool nightLeapFieldSpecified;

    private System.DateTime timeField;

    /// <remarks/>
    public int nightLeap {
        get { return this.nightLeapField; }
        set { this.nightLeapField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnore()]
    public bool nightLeapSpecified {
        get { return this.nightLeapFieldSpecified; }
        set { this.nightLeapFieldSpecified = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElement( DataType = "time" )]
    public System.DateTime time {
        get { return this.timeField; }
        set { this.timeField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainFineConstructionConstructionTrainSequenceSequenceServicePointPublishedArrival {
    private int nightLeapField;

    private bool nightLeapFieldSpecified;

    private System.DateTime timeField;

    /// <remarks/>
    public int nightLeap {
        get { return this.nightLeapField; }
        set { this.nightLeapField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnore()]
    public bool nightLeapSpecified {
        get { return this.nightLeapFieldSpecified; }
        set { this.nightLeapFieldSpecified = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElement( DataType = "time" )]
    public System.DateTime time {
        get { return this.timeField; }
        set { this.timeField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainFineConstructionConstructionTrainSequenceSequenceServicePointDeparture {
    private int nightLeapField;

    private bool nightLeapFieldSpecified;

    private System.DateTime timeField;

    /// <remarks/>
    public int nightLeap {
        get { return this.nightLeapField; }
        set { this.nightLeapField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnore()]
    public bool nightLeapSpecified {
        get { return this.nightLeapFieldSpecified; }
        set { this.nightLeapFieldSpecified = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElement( DataType = "time" )]
    public System.DateTime time {
        get { return this.timeField; }
        set { this.timeField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainFineConstructionConstructionTrainSequenceSequenceServicePointPublishedDeparture {
    private int nightLeapField;

    private bool nightLeapFieldSpecified;

    private System.DateTime timeField;

    /// <remarks/>
    public int nightLeap {
        get { return this.nightLeapField; }
        set { this.nightLeapField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnore()]
    public bool nightLeapSpecified {
        get { return this.nightLeapFieldSpecified; }
        set { this.nightLeapFieldSpecified = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElement( DataType = "time" )]
    public System.DateTime time {
        get { return this.timeField; }
        set { this.timeField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainFineConstructionConstructionTrainSequenceSequenceServicePointStopMode {
    private string typField;

    private int schluesselField;

    private string valueField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string Typ {
        get { return this.typField; }
        set { this.typField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public int Schluessel {
        get { return this.schluesselField; }
        set { this.schluesselField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlText()]
    public string Value {
        get { return this.valueField; }
        set { this.valueField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainFineConstructionConstructionTrainSequenceAnchor {
    private string anchorSequenceServicePointField;

    private System.DateTime anchorTimeField;

    private bool isDepartureTimeField;

    /// <remarks/>
    public string anchorSequenceServicePoint {
        get { return this.anchorSequenceServicePointField; }
        set { this.anchorSequenceServicePointField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElement( DataType = "time" )]
    public System.DateTime anchorTime {
        get { return this.anchorTimeField; }
        set { this.anchorTimeField = value; }
    }

    /// <remarks/>
    public bool isDepartureTime {
        get { return this.isDepartureTimeField; }
        set { this.isDepartureTimeField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainFineConstructionConnection {
    private string connectionModeField;

    private KSSRailmlTimetableTrainFineConstructionConnectionFirstTrain firstTrainField;

    private KSSRailmlTimetableTrainFineConstructionConnectionSecondTrain secondTrainField;

    private string firstServicePointField;

    private string secondServicePointField;

    private string timeValueField;

    private string statusField;

    /// <remarks/>
    public string connectionMode {
        get { return this.connectionModeField; }
        set { this.connectionModeField = value; }
    }

    /// <remarks/>
    public KSSRailmlTimetableTrainFineConstructionConnectionFirstTrain firstTrain {
        get { return this.firstTrainField; }
        set { this.firstTrainField = value; }
    }

    /// <remarks/>
    public KSSRailmlTimetableTrainFineConstructionConnectionSecondTrain secondTrain {
        get { return this.secondTrainField; }
        set { this.secondTrainField = value; }
    }

    /// <remarks/>
    public string firstServicePoint {
        get { return this.firstServicePointField; }
        set { this.firstServicePointField = value; }
    }

    /// <remarks/>
    public string secondServicePoint {
        get { return this.secondServicePointField; }
        set { this.secondServicePointField = value; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElement( DataType = "duration" )]
    public string timeValue {
        get { return this.timeValueField; }
        set { this.timeValueField = value; }
    }

    /// <remarks/>
    public string status {
        get { return this.statusField; }
        set { this.statusField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainFineConstructionConnectionFirstTrain {
    private int mainNumberField;

    private int subNumberField;

    private string userAbbreviationField;

    /// <remarks/>
    public int mainNumber {
        get { return this.mainNumberField; }
        set { this.mainNumberField = value; }
    }

    /// <remarks/>
    public int subNumber {
        get { return this.subNumberField; }
        set { this.subNumberField = value; }
    }

    /// <remarks/>
    public string userAbbreviation {
        get { return this.userAbbreviationField; }
        set { this.userAbbreviationField = value; }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute( "code" )]
[System.Xml.Serialization.XmlType( AnonymousType = true )]
public partial class KSSRailmlTimetableTrainFineConstructionConnectionSecondTrain {
    private int mainNumberField;

    private int subNumberField;

    private string userAbbreviationField;

    /// <remarks/>
    public int mainNumber {
        get { return this.mainNumberField; }
        set { this.mainNumberField = value; }
    }

    /// <remarks/>
    public int subNumber {
        get { return this.subNumberField; }
        set { this.subNumberField = value; }
    }

    /// <remarks/>
    public string userAbbreviation {
        get { return this.userAbbreviationField; }
        set { this.userAbbreviationField = value; }
    }
}
