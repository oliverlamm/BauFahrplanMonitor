using BauFahrplanMonitor.Api.Dto;
using BauFahrplanMonitor.Core.Importer.Helper;

namespace BauFahrplanMonitor.Core.Jobs;

public sealed class BbpNeoJobStatus {
    // -------------------------------------------------
    // Job-State
    // -------------------------------------------------
    public ImportJobState State { get; internal set; } = ImportJobState.Idle;

    public DateTime? StartedAt  { get; internal set; }
    public DateTime? FinishedAt { get; internal set; }

    public string? CurrentFile { get; internal set; }

    // -------------------------------------------------
    // Fortschritt (fachlich)
    // -------------------------------------------------
    public int MassnahmenGesamt => _massnahmenGesamt;
    public int MassnahmenFertig => _massnahmenFertig;

    public int Regelungen        => _regelungen;
    public int Betriebsverfahren => _betriebsverfahren;
    public int Aps               => _aps;
    public int Iav               => _iav;

    // -------------------------------------------------
    // Technisch
    // -------------------------------------------------
    public int Errors => _errors;

    // -------------------------------------------------
    // Intern (thread-safe)
    // -------------------------------------------------
    private int _massnahmenGesamt;
    private int _massnahmenFertig;

    private int _regelungen;
    private int _betriebsverfahren;
    private int _aps;
    private int _iav;

    private int _errors;

    // -------------------------------------------------
    // Reset
    // -------------------------------------------------
    internal void Reset() {
        State = ImportJobState.Idle;

        StartedAt   = null;
        FinishedAt  = null;
        CurrentFile = null;

        _massnahmenGesamt  = 0;
        _massnahmenFertig  = 0;
        _regelungen        = 0;
        _betriebsverfahren = 0;
        _aps               = 0;
        _iav               = 0;
        _errors            = 0;
    }

    // -------------------------------------------------
    // Fehler
    // -------------------------------------------------
    internal void IncrementErrors()
        => Interlocked.Increment(ref _errors);

    // -------------------------------------------------
    // Progress-Update aus Importer
    // -------------------------------------------------
    internal void UpdateFrom(ImportProgressInfo info) {
        if (info == null)
            return;

        // ⚠️ hier NICHT addieren, sondern setzen
        // Importer liefert bereits kumulative Werte
        _massnahmenGesamt = info.MassnahmenGesamt;
        _massnahmenFertig = info.MassnahmenFertig;

        _regelungen        = info.Regelungen;
        _betriebsverfahren = info.Betriebsverfahren;
        _aps               = info.APS;
        _iav               = info.IAV;
    }

    // -------------------------------------------------
    // Abschluss
    // -------------------------------------------------
    internal void MarkFinished(bool withErrors) {
        FinishedAt = DateTime.UtcNow;
        State = withErrors
            ? ImportJobState.FinishedWithErrors
            : ImportJobState.Finished;
    }

    public BbpNeoJobStatusDto ToDto() {
        return new BbpNeoJobStatusDto {
            State       = State,
            StartedAt   = StartedAt,
            FinishedAt  = FinishedAt,
            CurrentFile = CurrentFile,

            MassnahmenGesamt = _massnahmenGesamt,
            MassnahmenFertig = _massnahmenFertig,

            Regelungen        = _regelungen,
            Betriebsverfahren = _betriebsverfahren,
            Aps               = _aps,
            Iav               = _iav,

            Errors = _errors
        };
    }

}