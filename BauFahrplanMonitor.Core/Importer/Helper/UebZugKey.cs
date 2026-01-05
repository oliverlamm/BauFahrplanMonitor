using System;

public readonly record struct UebZugKey(
    long     ZugNr,
    DateOnly Verkehrstag);