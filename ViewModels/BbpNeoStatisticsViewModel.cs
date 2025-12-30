using BauFahrplanMonitor.Importer.Helper;

namespace BauFahrplanMonitor.ViewModels;

public sealed class BbpNeoStatisticsViewModel : ImportStatisticsViewModel {
    private int _massnahmen;

    public int Massnahmen {
        get => _massnahmen;
        private set => SetProperty(ref _massnahmen, value);
    }

    private int _regelungen;

    public int Regelungen {
        get => _regelungen;
        private set => SetProperty(ref _regelungen, value);
    }

    private int _bve;

    public int BvE {
        get => _bve;
        private set => SetProperty(ref _bve, value);
    }

    private int _aps;

    public int APS {
        get => _aps;
        private set => SetProperty(ref _aps, value);
    }

    private int _iav;

    public int IAV {
        get => _iav;
        private set => SetProperty(ref _iav, value);
    }

    public override void Reset() {
        Massnahmen = Regelungen = BvE = APS = IAV = 0;
    }

    public override void Update(ImportProgressInfo info) {
        Massnahmen = info.MeasuresDone;
        Regelungen = info.Regelungen;
        BvE        = info.BvE;
        APS        = info.APS;
        IAV        = info.IAV;
    }
}