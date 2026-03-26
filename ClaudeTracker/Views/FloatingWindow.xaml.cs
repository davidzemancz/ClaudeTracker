using System.Windows;
using System.Windows.Input;

namespace ClaudeTracker.Views;

public partial class FloatingWindow : Window
{
    public FloatingWindow()
    {
        InitializeComponent();

        // Position at top-right of primary screen
        var area = SystemParameters.WorkArea;
        Left = area.Right - Width - 20;
        Top = area.Top + 20;
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }
}
