namespace BauFahrplanMonitor.Configuration;

public class AllgemeinConfig {
    public int  ImportThreads      { get; set; }
    public bool Debugging          { get; set; }
    public bool StopAfterException { get; set; }
}