namespace BauFahrplanMonitor.Core.Domain.Infrastruktur;

public sealed class MutterBetriebsstelle {
    
        public string  Ds100 { get; init; } = null!;
        public string  Langname  { get; init; } = null!;
        
        public double? Breite { get; init; }
        public double? Laenge { get; init; }

        public List<string>? Tochterbetriebsstellen { get; set; } = null;
}