using BauFahrplanMonitor.Core.Importer.Dto.Nfpl;
using BauFahrplanMonitor.Core.Importer.Xml;
using BauFahrplanMonitor.Core.Interfaces;


namespace BauFahrplanMonitor.Core.Importer.Mapper;

public sealed class NetzfahrplanMapper : INetzfahrplanMapper {
    public NetzfahrplanDto Map(
        KSSxmlSchnittstelle xml,
        string              filePath) {
        ArgumentNullException.ThrowIfNull(xml);

        var fahrplanJahr =
            xml.header
                .description
                .scheduleVersion
                .yearTimetable;

        var dto = new NetzfahrplanDto {
            FahrplanJahr = fahrplanJahr,
            Region       = xml.header.establishment
        };

        if (xml.railml?.timetable?.train == null)
            return dto;

        // -------------------------------------------------
        // Züge gruppieren nach Zugnummer
        // -------------------------------------------------
        foreach (var train in xml.railml.timetable.train) {

            var zugNr = ExtractZugNr(train);
            if (zugNr == null)
                continue;

            var zugDto = dto.Zuege.FirstOrDefault(z => z.ZugNr == zugNr.Value);
            if (zugDto == null) {
                zugDto = new NetzfahrplanZugDto {
                    ZugNr = zugNr.Value
                };
                dto.Zuege.Add(zugDto);
            }

            zugDto.Varianten.Add(MapVariante(train, dto.Region));
        }

        return dto;
    }

    // =====================================================================
    // Variante
    // =====================================================================

    private static NetzfahrplanZugVarianteDto MapVariante(
        KSSRailmlTimetableTrain train, string? region) {
        var variante = new NetzfahrplanZugVarianteDto {
            TrainId     = train.trainID,
            TrainNumber = train.trainNumber,
            Kind        = train.kind,
            TrainStatus = train.trainStatus,
            Remarks     = train.remarks,
            Region      = region
        };

        if (train.timetableentries == null)
            return variante;

        var  serviceState = new ServiceState();
        long seq          = 1;

        foreach (var entry in train.timetableentries) {

            // 🔁 Service aktualisieren, falls vorhanden
            TryUpdateServiceState(entry, serviceState);

            variante.Verlauf.Add(
                new NetzfahrplanVerlaufDto {
                    Seq      = seq++,
                    BstRl100 = entry.posID,
                    Type     = entry.type,

                    PublishedArrival = entry.publishedArrivalSpecified
                            ? TimeOnly.FromDateTime(entry.publishedArrival)
                            : null,

                    PublishedDeparture = entry.publishedDepartureSpecified
                            ? TimeOnly.FromDateTime(entry.publishedDeparture)
                            : default,

                    Remarks = entry.remarks,

                    ServiceBitmask     = serviceState.Bitmask,
                    ServiceStartdate   = serviceState.Start,
                    ServiceEnddate     = serviceState.End,
                    ServiceDescription = serviceState.Description
                });
        }

        return variante;
    }

    // =====================================================================
    // Service-Vererbung (sticky)
    // =====================================================================

    private static void TryUpdateServiceState(
        KSSRailmlTimetableTrainEntry entry,
        ServiceState                 state) {
        var basicChar =
            entry.additionalInformation?
                .Items?
                .OfType<KSSRailmlTimetableTrainEntryAdditionalInformationBasicCharacteristic>()
                .FirstOrDefault();

        var svc = basicChar?.service;
        if (svc == null)
            return;

        state.Bitmask     = svc.bitMask;
        state.Start       = DateOnly.FromDateTime(svc.startDate);
        state.End         = DateOnly.FromDateTime(svc.endDate);
        state.Description = svc.description;
    }

    // =====================================================================
    // Hilfsfunktionen
    // =====================================================================

    private static long? ExtractZugNr(
        KSSRailmlTimetableTrain train) {
        // 1) direkt aus trainNumber (z. B. "525", "525/1-SF1")
        if (!string.IsNullOrWhiteSpace(train.trainNumber)) {
            var digits = new string(train.trainNumber.TakeWhile(char.IsDigit).ToArray());

            if (long.TryParse(digits, out var nr))
                return nr;
        }

        // 2) Fallback: fineConstruction / constructionTrain
        var ct =
            train.fineConstruction?
                .constructionTrain?
                .FirstOrDefault();

        return ct?.trainNumber?.mainNumber;
    }

    // =====================================================================
    // interner Service-State
    // =====================================================================

    private sealed class ServiceState {
        public string?   Bitmask;
        public DateOnly? Start;
        public DateOnly? End;
        public string?   Description;
    }
}