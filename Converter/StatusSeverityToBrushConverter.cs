using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using BauFahrplanMonitor.Helpers;

namespace BauFahrplanMonitor.Converter;

public sealed class StatusSeverityToBrushConverter : IValueConverter {
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value switch {
            StatusSeverity.Success => Brushes.ForestGreen,
            StatusSeverity.Warning => Brushes.DarkOrange,
            StatusSeverity.Error   => Brushes.IndianRed,
            _                      => Brushes.Black
        };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}