using System.IO;
using Avalonia.Media.Imaging;
using SkiaSharp;

namespace ClaudeTracker.Services;

public static class IconGenerator
{
    public static Bitmap CreatePercentIcon(double percent)
    {
        const int size = 16;
        using var surface = SKSurface.Create(new SKImageInfo(size, size));
        var canvas = surface.Canvas;

        // Background color based on percent
        var bgColor = percent switch
        {
            < 50 => new SKColor(0x4C, 0xAF, 0x50),  // Green
            < 80 => new SKColor(0xFF, 0xC1, 0x07),   // Yellow
            _ => new SKColor(0xF4, 0x43, 0x36)        // Red
        };

        using var bgPaint = new SKPaint { Color = bgColor };
        canvas.DrawRect(0, 0, size, size, bgPaint);

        // Text
        var text = $"{percent:F0}";
        var fontSize = text.Length > 2 ? 8f : 10f;

        var fontFamily = OperatingSystem.IsMacOS() ? "Helvetica Neue" : "Segoe UI";
        using var typeface = SKTypeface.FromFamilyName(fontFamily, SKFontStyle.Bold)
                          ?? SKTypeface.Default;
        using var textPaint = new SKPaint
        {
            Color = SKColors.White,
            IsAntialias = true,
            TextSize = fontSize,
            Typeface = typeface
        };

        // Measure and center text
        var textBounds = new SKRect();
        textPaint.MeasureText(text, ref textBounds);
        var x = (size - textBounds.Width) / 2 - textBounds.Left;
        var y = (size - textBounds.Height) / 2 - textBounds.Top;

        canvas.DrawText(text, x, y, textPaint);
        canvas.Flush();

        // Convert to Avalonia Bitmap
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var ms = new MemoryStream(data.ToArray());
        return new Bitmap(ms);
    }
}
