using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace ClaudeTracker.Views;

public partial class FloatingWindow : Window
{
    public FloatingWindow()
    {
        InitializeComponent();

        // Position at top-right of primary screen once the window is opened
        Opened += (_, _) =>
        {
            var screen = Screens.Primary;
            if (screen == null) return;
            var workArea = screen.WorkingArea;
            var scaling = screen.Scaling;
            Position = new PixelPoint(
                (int)(workArea.Right / scaling - Width - 20),
                (int)(workArea.Y / scaling + 20));
        };
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }
}
