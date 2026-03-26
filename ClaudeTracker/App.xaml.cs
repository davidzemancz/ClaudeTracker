using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using ClaudeTracker.Services;
using ClaudeTracker.ViewModels;
using ClaudeTracker.Views;
using Hardcodet.Wpf.TaskbarNotification;

namespace ClaudeTracker;

public partial class App : Application
{
    private TaskbarIcon? _trayIcon;
    private StatsDataService? _statsService;
    private UsageApiService? _usageService;
    private StatsFileWatcher? _fileWatcher;
    private DispatcherTimer? _apiPollTimer;
    private DispatcherTimer? _fallbackPollTimer;
    private TrayViewModel? _trayViewModel;
    private FloatingWindow? _floatingWindow;
    private Mutex? _singleInstanceMutex;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Single instance guard
        _singleInstanceMutex = new Mutex(true, "ClaudeTrackerSingleInstance", out bool createdNew);
        if (!createdNew)
        {
            MessageBox.Show("Claude Tracker is already running.", "Claude Tracker",
                MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }

        // Initialize services
        _statsService = new StatsDataService();
        _usageService = new UsageApiService();

        _statsService.ReloadStats();
        _statsService.ReloadSessions();

        // Initialize file watcher
        _fileWatcher = new StatsFileWatcher(_statsService);

        // Create view model
        _trayViewModel = new TrayViewModel(_statsService, _usageService);
        _trayViewModel.FloatRequested += ToggleFloatingWindow;

        // Subscribe to usage updates to refresh the tray icon
        _usageService.UsageUpdated += UpdateTrayIcon;

        // Create tray icon
        var popup = new TrayPopup { DataContext = _trayViewModel };
        _trayIcon = new TaskbarIcon
        {
            ToolTipText = "Claude Code Tracker",
            TrayPopup = popup
        };
        UpdateTrayIcon();

        // API polling timer (every 150 seconds)
        _apiPollTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(150) };
        _apiPollTimer.Tick += async (_, _) => await _usageService.FetchUsageAsync();
        _apiPollTimer.Start();

        // Fallback local stats poll (every 30 seconds)
        _fallbackPollTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        _fallbackPollTimer.Tick += (_, _) =>
        {
            _statsService.ReloadStats();
            _statsService.ReloadSessions();
        };
        _fallbackPollTimer.Start();

        // Initial API fetch
        await _usageService.FetchUsageAsync();
    }

    private void ToggleFloatingWindow()
    {
        if (_floatingWindow != null)
        {
            _floatingWindow.Close();
            _floatingWindow = null;
            return;
        }

        var vm = new FloatingViewModel(_usageService!, _statsService!);
        _floatingWindow = new FloatingWindow { DataContext = vm };
        vm.CloseRequested += () =>
        {
            _floatingWindow?.Close();
            _floatingWindow = null;
        };
        _floatingWindow.Closed += (_, _) => _floatingWindow = null;
        _floatingWindow.Show();
    }

    private void UpdateTrayIcon()
    {
        if (_trayIcon == null || _trayViewModel == null) return;

        var percent = _trayViewModel.GetIconPercent();
        _trayIcon.Icon = CreatePercentIcon(percent);
        _trayIcon.ToolTipText = _trayViewModel.GetTooltipText();
    }

    private static System.Drawing.Icon CreatePercentIcon(double percent)
    {
        using var bmp = new Bitmap(16, 16);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

        // Background color based on percent
        var bgColor = percent switch
        {
            < 50 => Color.FromArgb(76, 175, 80),    // Green
            < 80 => Color.FromArgb(255, 193, 7),     // Yellow
            _ => Color.FromArgb(244, 67, 54)          // Red
        };

        using var bgBrush = new SolidBrush(bgColor);
        g.FillRectangle(bgBrush, 0, 0, 16, 16);

        // Text
        var text = $"{percent:F0}";
        using var font = new Font("Segoe UI", text.Length > 2 ? 6.5f : 8f, System.Drawing.FontStyle.Bold);
        using var textBrush = new SolidBrush(Color.White);
        using var sf = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        g.DrawString(text, font, textBrush, new RectangleF(0, 0, 16, 16), sf);

        var handle = bmp.GetHicon();
        return System.Drawing.Icon.FromHandle(handle);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _apiPollTimer?.Stop();
        _fallbackPollTimer?.Stop();
        _fileWatcher?.Dispose();
        _usageService?.Dispose();
        _trayIcon?.Dispose();
        _singleInstanceMutex?.Dispose();
        _floatingWindow?.Close();
        base.OnExit(e);
    }
}
