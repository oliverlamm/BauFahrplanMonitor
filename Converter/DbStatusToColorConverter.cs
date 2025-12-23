using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace BauFahrplanMonitor.Converter;

public class DbStatusToColorConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        // Hier definieren wir die Farben basierend auf dem DB-Verbindungsstatus
        if (value is bool isConnected) {
            return isConnected ? Brushes.LightGreen : Brushes.LightCoral; // Grün für verbunden, Rot für nicht verbunden
        }

        // Standardmäßig Weiß, wenn der Status noch nicht bekannt ist
        return Brushes.White;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}