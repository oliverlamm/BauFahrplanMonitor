namespace BauFahrplanMonitor.Core.Jobs;

/// <summary>
/// Fortschrittsstatus eines Trassenfinder-Refresh-Jobs
/// </summary>
public sealed class TrassenfinderInfraStatus
{
    /// <summary>
    /// Fortschritt in Prozent (0–100)
    /// </summary>
    public int Percent { get; init; }

    /// <summary>
    /// Lesbare Statusmeldung für UI / Logs
    /// </summary>
    public string Message { get; init; } = string.Empty;
}