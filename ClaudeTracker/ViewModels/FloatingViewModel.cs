using System.Windows.Input;
using ClaudeTracker.Services;

namespace ClaudeTracker.ViewModels;

public class FloatingViewModel : ViewModelBase
{
    private readonly UsageApiService _usageService;
    private readonly StatsDataService _statsService;

    private double _fiveHourPercent;
    private double _sevenDayPercent;
    private string _fiveHourResetText = "---";
    private string _activeSessions = "0";

    public double FiveHourPercent { get => _fiveHourPercent; set => SetField(ref _fiveHourPercent, value); }
    public double SevenDayPercent { get => _sevenDayPercent; set => SetField(ref _sevenDayPercent, value); }
    public string FiveHourResetText { get => _fiveHourResetText; set => SetField(ref _fiveHourResetText, value); }
    public string ActiveSessions { get => _activeSessions; set => SetField(ref _activeSessions, value); }

    public ICommand CloseCommand { get; }

    public event Action? CloseRequested;

    public FloatingViewModel(UsageApiService usageService, StatsDataService statsService)
    {
        _usageService = usageService;
        _statsService = statsService;

        _usageService.UsageUpdated += Refresh;
        _statsService.DataChanged += RefreshSessions;

        CloseCommand = new RelayCommand(() => CloseRequested?.Invoke());

        Refresh();
        RefreshSessions();
    }

    private void Refresh()
    {
        var usage = _usageService.CurrentUsage;
        if (usage == null) return;

        FiveHourPercent = usage.FiveHour?.Utilization ?? 0;
        SevenDayPercent = usage.SevenDay?.Utilization ?? 0;

        var resetsAt = usage.FiveHour?.ResetsAt;
        if (resetsAt != null)
        {
            var diff = resetsAt.Value.ToLocalTime() - DateTime.Now;
            FiveHourResetText = diff.TotalSeconds < 0 ? "now" :
                diff.TotalMinutes >= 60 ? $"{(int)diff.TotalHours}h {diff.Minutes}m" :
                $"{(int)diff.TotalMinutes}m";
        }
    }

    private void RefreshSessions()
    {
        ActiveSessions = _statsService.ActiveSessions.Count.ToString();
    }
}
