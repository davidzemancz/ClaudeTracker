using System.Globalization;
using Avalonia.Data.Converters;

namespace ClaudeTracker.Converters;

public class PercentToWidthConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var percent = value is double d ? d : 0;
        var maxWidth = double.Parse((string?)parameter ?? "260", CultureInfo.InvariantCulture);
        return Math.Max(0, Math.Min(maxWidth, percent / 100.0 * maxWidth));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
