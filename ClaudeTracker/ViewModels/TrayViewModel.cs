using System.Windows;
using System.Windows.Input;
using ClaudeTracker.Services;
using Microsoft.Win32;

namespace ClaudeTracker.ViewModels;

public class TrayViewModel : ViewModelBase
{
    private readonly StatsDataService _statsService;
    private readonly UsageApiService _usageService;

    // Rate limits
    private double _fiveHourPercent;
    private double _sevenDayPercent;
    private string _fiveHourResetText = "---";
    private string _sevenDayResetText = "---";
    private string _lastUpdateText = "Never";
    private string _errorText = "";

    // Local stats
    private string _todayMessages = "0";
    private string _todayTools = "0";
    private string _todaySessions = "0";
    private string _weekMessages = "0";
    private string _weekTools = "0";
    private string _allTimeMessages = "0";
    private string _allTimeSessions = "0";
    private string _sinceDate = "---";
    private string _activeSessions = "0";
    private string _modelsSummary = "";
    private bool _startWithWindows;

    public double FiveHourPercent { get => _fiveHourPercent; set => SetField(ref _fiveHourPercent, value); }
    public double SevenDayPercent { get => _sevenDayPercent; set => SetField(ref _sevenDayPercent, value); }
    public string FiveHourResetText { get => _fiveHourResetText; set => SetField(ref _fiveHourResetText, value); }
    public string SevenDayResetText { get => _sevenDayResetText; set => SetField(ref _sevenDayResetText, value); }
    public string LastUpdateText { get => _lastUpdateText; set => SetField(ref _lastUpdateText, value); }
    public string ErrorText { get => _errorText; set => SetField(ref _errorText, value); }

    public string TodayMessages { get => _todayMessages; set => SetField(ref _todayMessages, value); }
    public string TodayTools { get => _todayTools; set => SetField(ref _todayTools, value); }
    public string TodaySessions { get => _todaySessions; set => SetField(ref _todaySessions, value); }
    public string WeekMessages { get => _weekMessages; set => SetField(ref _weekMessages, value); }
    public string WeekTools { get => _weekTools; set => SetField(ref _weekTools, value); }
    public string AllTimeMessages { get => _allTimeMessages; set => SetField(ref _allTimeMessages, value); }
    public string AllTimeSessions { get => _allTimeSessions; set => SetField(ref _allTimeSessions, value); }
    public string SinceDate { get => _sinceDate; set => SetField(ref _sinceDate, value); }
    public string ActiveSessions { get => _activeSessions; set => SetField(ref _activeSessions, value); }
    public string ModelsSummary { get => _modelsSummary; set => SetField(ref _modelsSummary, value); }
    public bool StartWithWindows
    {
        get => _startWithWindows;
        set
        {
            if (SetField(ref _startWithWindows, value))
                SetAutoStart(value);
        }
    }

    public ICommand FloatCommand { get; }
    public ICommand ExitCommand { get; }
    public ICommand RefreshCommand { get; }

    public event Action? FloatRequested;

    public TrayViewModel(StatsDataService statsService, UsageApiService usageService)
    {
        _statsService = statsService;
        _usageService = usageService;

        _statsService.DataChanged += RefreshLocalStats;
        _usageService.UsageUpdated += RefreshUsageData;

        FloatCommand = new RelayCommand(() => FloatRequested?.Invoke());
        ExitCommand = new RelayCommand(() => Application.Current.Shutdown());
        RefreshCommand = new RelayCommand(async () => await _usageService.FetchUsageAsync());

        _startWithWindows = IsAutoStartEnabled();

        RefreshLocalStats();
        RefreshUsageData();
    }

    private void RefreshUsageData()
    {
        var usage = _usageService.CurrentUsage;
        if (usage != null)
        {
            FiveHourPercent = usage.FiveHour?.Utilization ?? 0;
            SevenDayPercent = usage.SevenDay?.Utilization ?? 0;
            FiveHourResetText = FormatResetTime(usage.FiveHour?.ResetsAt);
            SevenDayResetText = FormatResetTime(usage.SevenDay?.ResetsAt);
        }

        if (_usageService.LastFetchTime != DateTime.MinValue)
            LastUpdateText = _usageService.LastFetchTime.ToString("HH:mm:ss");

        ErrorText = _usageService.LastError ?? "";
    }

    private void RefreshLocalStats()
    {
        var today = _statsService.TodayActivity;
        TodayMessages = StatsDataService.FormatNumber(today?.MessageCount ?? 0);
        TodayTools = StatsDataService.FormatNumber(today?.ToolCallCount ?? 0);
        TodaySessions = StatsDataService.FormatNumber(today?.SessionCount ?? 0);

        var week = _statsService.GetThisWeekActivity();
        WeekMessages = StatsDataService.FormatNumber(week.MessageCount);
        WeekTools = StatsDataService.FormatNumber(week.ToolCallCount);

        var stats = _statsService.Stats;
        if (stats != null)
        {
            AllTimeMessages = StatsDataService.FormatNumber(stats.TotalMessages);
            AllTimeSessions = StatsDataService.FormatNumber(stats.TotalSessions);

            if (DateTime.TryParse(stats.FirstSessionDate, out var firstDate))
                SinceDate = firstDate.ToString("MMM dd, yyyy");

            var models = stats.ModelUsage
                .Select(kv =>
                {
                    var name = kv.Key.Replace("claude-", "").Replace("-2025", "");
                    var total = kv.Value.InputTokens + kv.Value.OutputTokens;
                    return $"{name}: {StatsDataService.FormatTokens(total)}";
                });
            ModelsSummary = string.Join("\n", models);
        }

        ActiveSessions = _statsService.ActiveSessions.Count.ToString();
    }

    private static string FormatResetTime(DateTime? resetsAt)
    {
        if (resetsAt == null) return "---";
        var diff = resetsAt.Value.ToLocalTime() - DateTime.Now;
        if (diff.TotalSeconds < 0) return "now";
        if (diff.TotalHours >= 24) return $"{(int)diff.TotalDays}d {diff.Hours}h";
        if (diff.TotalMinutes >= 60) return $"{(int)diff.TotalHours}h {diff.Minutes}m";
        return $"{(int)diff.TotalMinutes}m";
    }

    public string GetTooltipText()
    {
        return $"Claude: {FiveHourPercent:F0}% (5h) | {SevenDayPercent:F0}% (7d) | Resets {FiveHourResetText}";
    }

    public double GetIconPercent() => FiveHourPercent;

    private const string AutoStartKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "ClaudeTracker";

    private static bool IsAutoStartEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(AutoStartKey, false);
        return key?.GetValue(AppName) != null;
    }

    private static void SetAutoStart(bool enable)
    {
        using var key = Registry.CurrentUser.OpenSubKey(AutoStartKey, true);
        if (key == null) return;

        if (enable)
        {
            var exePath = Environment.ProcessPath ?? "";
            key.SetValue(AppName, $"\"{exePath}\"");
        }
        else
        {
            key.DeleteValue(AppName, false);
        }
    }
}
