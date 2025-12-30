using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace BauFahrplanMonitor.Views.Controls;

public partial class StatRow : UserControl {
    public static readonly StyledProperty<string> LabelProperty =
        AvaloniaProperty.Register<StatRow, string>(nameof(Label), string.Empty);

    public static readonly StyledProperty<object?> ValueProperty =
        AvaloniaProperty.Register<StatRow, object?>(nameof(Value));

    public string Label {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public object? Value {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public StatRow() {
        InitializeComponent();
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}