using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace BauFahrplanMonitor.Converter;

public sealed class BoolToXConverter : IValueConverter {
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? "X" : "";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => BindingOperations.DoNothing;
}