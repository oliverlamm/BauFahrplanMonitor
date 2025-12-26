using System;

namespace BauFahrplanMonitor.Importer.Helper;

public readonly record struct FploZugKey(
    long     ZugNr,
    DateOnly Verkehrstag);