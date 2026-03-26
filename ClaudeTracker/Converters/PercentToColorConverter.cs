using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace ClaudeTracker.Converters;

public class PercentToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var percent = value is double d ? d : 0;
        return percent switch
        {
            < 50 => new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50)),  // Green
            < 80 => new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x07)),  // Yellow
            _ => new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36))      // Red
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
