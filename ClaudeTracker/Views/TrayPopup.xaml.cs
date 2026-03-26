using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using WpfColor = System.Windows.Media.Color;

namespace ClaudeTracker.Views;

public partial class TrayPopup : UserControl
{
    public TrayPopup()
    {
        InitializeComponent();
    }
}

public class PercentToWidthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var percent = (double)value;
        var maxWidth = double.Parse((string)parameter, CultureInfo.InvariantCulture);
        return Math.Max(0, Math.Min(maxWidth, percent / 100.0 * maxWidth));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class PercentToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var percent = (double)value;
        return percent switch
        {
            < 50 => new SolidColorBrush(WpfColor.FromRgb(0x4C, 0xAF, 0x50)),  // Green
            < 80 => new SolidColorBrush(WpfColor.FromRgb(0xFF, 0xC1, 0x07)),  // Yellow
            _ => new SolidColorBrush(WpfColor.FromRgb(0xF4, 0x43, 0x36))      // Red
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class StringToVisibilityConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
