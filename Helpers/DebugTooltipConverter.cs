using System;
using System.Globalization;
using Avalonia.Data.Converters;

public class DebugTooltipConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true
            ? "Aktiviert"
            : "Deaktiviert";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}