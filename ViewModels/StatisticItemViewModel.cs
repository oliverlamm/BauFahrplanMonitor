using CommunityToolkit.Mvvm.ComponentModel;

namespace BauFahrplanMonitor.ViewModels;

public sealed class StatisticItemViewModel : ObservableObject {
    public string Key { get; }

    private string _value;

    public string Value {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    public StatisticItemViewModel(string key, string value = "") {
        Key    = key;
        _value = value;
    }
}