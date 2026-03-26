using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ClaudeTracker.Services;
using ClaudeTracker.ViewModels;
using ClaudeTracker.Views;

namespace ClaudeTracker;

public partial class App : Application
{
    private StatsDataService? _statsService;
    private UsageApiService? _usageService;
    private StatsFileWatcher? _fileWatcher;
    private IAutoStartService? _autoStartService;
    private DispatcherTimer? _apiPollTimer;
    private DispatcherTimer? _fallbackPollTimer;
    private TrayViewModel? _trayViewModel;
    private FloatingWindow? _floatingWindow;
    private TrayIcon? _trayIcon;
    private Mutex? _singleInstanceMutex;

    public override void Initialize()
    {
        Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        base.OnFrameworkInitializationCompleted();

        // Single instance guard
        _singleInstanceMutex = new Mutex(true, "ClaudeTrackerSingleInstance", out bool createdNew);
        if (!createdNew)
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                desktop.Shutdown();
            return;
        }

        // Initialize services
        _statsService = new StatsDataService();
        _usageService = new UsageApiService();
        _autoStartService = OperatingSystem.IsMacOS()
            ? new MacAutoStartService()
            : new WindowsAutoStartService();

        _statsService.ReloadStats();
        _statsService.ReloadSessions();

        // Initialize file watcher
        _fileWatcher = new StatsFileWatcher(_statsService);

        // Create view model
        _trayViewModel = new TrayViewModel(_statsService, _usageService, _autoStartService);
        _trayViewModel.FloatRequested += ToggleFloatingWindow;

        // Subscribe to usage updates to refresh the tray icon
        _usageService.UsageUpdated += UpdateTrayIcon;

        // Create tray icon
        SetupTrayIcon();
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

    private void SetupTrayIcon()
    {
        var menu = new NativeMenu();

        var statsItem = new NativeMenuItem { Header = "Claude Code Tracker", IsEnabled = false };
        menu.Items.Add(statsItem);
        menu.Items.Add(new NativeMenuItemSeparator());

        var floatItem = new NativeMenuItem { Header = "Float" };
        floatItem.Click += (_, _) => ToggleFloatingWindow();
        menu.Items.Add(floatItem);

        var refreshItem = new NativeMenuItem { Header = "Refresh" };
        refreshItem.Click += async (_, _) => await _usageService!.FetchUsageAsync();
        menu.Items.Add(refreshItem);

        menu.Items.Add(new NativeMenuItemSeparator());

        var exitItem = new NativeMenuItem { Header = "Exit" };
        exitItem.Click += (_, _) =>
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                desktop.Shutdown();
        };
        menu.Items.Add(exitItem);

        _trayIcon = new TrayIcon
        {
            ToolTipText = "Claude Code Tracker",
            Menu = menu,
            IsVisible = true
        };

        // On Windows, clicking the tray icon toggles the popup window
        // On macOS, Clicked is not supported - users use the NativeMenu instead
        _trayIcon.Clicked += (_, _) => ToggleFloatingWindow();
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
        _trayIcon.Icon = new WindowIcon(IconGenerator.CreatePercentIcon(percent));
        _trayIcon.ToolTipText = _trayViewModel.GetTooltipText();

        // Update the stats line in the native menu
        if (_trayIcon.Menu?.Items.Count > 0 && _trayIcon.Menu.Items[0] is NativeMenuItem statsItem)
        {
            statsItem.Header = $"{percent:F0}% (5h) | {_trayViewModel.SevenDayPercent:F0}% (7d)";
        }
    }

    private void OnExit()
    {
        _apiPollTimer?.Stop();
        _fallbackPollTimer?.Stop();
        _fileWatcher?.Dispose();
        _usageService?.Dispose();
        _trayIcon?.Dispose();
        _singleInstanceMutex?.Dispose();
        _floatingWindow?.Close();
    }
}
