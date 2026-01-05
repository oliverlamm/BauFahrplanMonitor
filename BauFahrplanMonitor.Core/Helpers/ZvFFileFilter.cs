namespace BauFahrplanMonitor.Core.Helpers;

[Flags]
public enum ZvFFileFilter {
    None = 0,
    ZvF  = 1,
    UeB  = 2,
    Fplo = 4,
    All  = ZvF | UeB | Fplo
}