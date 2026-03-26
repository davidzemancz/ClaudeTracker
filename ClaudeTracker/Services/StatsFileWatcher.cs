using System.IO;

namespace ClaudeTracker.Services;

public class StatsFileWatcher : IDisposable
{
    private readonly StatsDataService _dataService;
    private readonly FileSystemWatcher? _statsWatcher;
    private readonly FileSystemWatcher? _sessionsWatcher;
    private System.Threading.Timer? _statsDebounce;
    private System.Threading.Timer? _sessionsDebounce;

    private static readonly string ClaudeDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".claude");

    public StatsFileWatcher(StatsDataService dataService)
    {
        _dataService = dataService;

        var statsFile = Path.Combine(ClaudeDir, "stats-cache.json");
        if (File.Exists(statsFile))
        {
            _statsWatcher = new FileSystemWatcher(ClaudeDir, "stats-cache.json")
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                EnableRaisingEvents = true
            };
            _statsWatcher.Changed += OnStatsChanged;
            _statsWatcher.Created += OnStatsChanged;
        }

        var sessionsDir = Path.Combine(ClaudeDir, "sessions");
        if (Directory.Exists(sessionsDir))
        {
            _sessionsWatcher = new FileSystemWatcher(sessionsDir, "*.json")
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                EnableRaisingEvents = true
            };
            _sessionsWatcher.Changed += OnSessionsChanged;
            _sessionsWatcher.Created += OnSessionsChanged;
            _sessionsWatcher.Deleted += OnSessionsChanged;
        }
    }

    private void OnStatsChanged(object sender, FileSystemEventArgs e)
    {
        _statsDebounce?.Dispose();
        _statsDebounce = new System.Threading.Timer(_ =>
        {
            System.Windows.Application.Current?.Dispatcher.InvokeAsync(() =>
                _dataService.ReloadStats());
        }, null, 500, Timeout.Infinite);
    }

    private void OnSessionsChanged(object sender, FileSystemEventArgs e)
    {
        _sessionsDebounce?.Dispose();
        _sessionsDebounce = new System.Threading.Timer(_ =>
        {
            System.Windows.Application.Current?.Dispatcher.InvokeAsync(() =>
                _dataService.ReloadSessions());
        }, null, 500, Timeout.Infinite);
    }

    public void Dispose()
    {
        _statsWatcher?.Dispose();
        _sessionsWatcher?.Dispose();
        _statsDebounce?.Dispose();
        _sessionsDebounce?.Dispose();
    }
}
