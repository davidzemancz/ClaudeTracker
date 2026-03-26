using System.Diagnostics;
using System.IO;
using System.Text.Json;
using ClaudeTracker.Models;

namespace ClaudeTracker.Services;

public class StatsDataService
{
    private static readonly string ClaudeDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".claude");

    private static readonly string StatsCachePath = Path.Combine(ClaudeDir, "stats-cache.json");
    private static readonly string SessionsDir = Path.Combine(ClaudeDir, "sessions");

    public StatsCache? Stats { get; private set; }
    public List<SessionInfo> ActiveSessions { get; private set; } = [];

    public event Action? DataChanged;

    public DailyActivityEntry? TodayActivity =>
        Stats?.DailyActivity.FirstOrDefault(a => a.Date == DateTime.Today.ToString("yyyy-MM-dd"));

    public DailyActivityEntry GetThisWeekActivity()
    {
        if (Stats == null) return new DailyActivityEntry();

        var today = DateTime.Today;
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
        if (today.DayOfWeek == DayOfWeek.Sunday) startOfWeek = startOfWeek.AddDays(-7);

        var weekEntries = Stats.DailyActivity
            .Where(a => DateTime.TryParse(a.Date, out var d) && d >= startOfWeek && d <= today)
            .ToList();

        return new DailyActivityEntry
        {
            MessageCount = weekEntries.Sum(e => e.MessageCount),
            SessionCount = weekEntries.Sum(e => e.SessionCount),
            ToolCallCount = weekEntries.Sum(e => e.ToolCallCount)
        };
    }

    public void ReloadStats()
    {
        try
        {
            if (!File.Exists(StatsCachePath)) return;

            using var stream = new FileStream(StatsCachePath,
                FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            Stats = JsonSerializer.Deserialize<StatsCache>(stream);
            DataChanged?.Invoke();
        }
        catch (IOException) { }
        catch (JsonException) { }
    }

    public void ReloadSessions()
    {
        try
        {
            if (!Directory.Exists(SessionsDir))
            {
                ActiveSessions = [];
                DataChanged?.Invoke();
                return;
            }

            var sessions = new List<SessionInfo>();
            foreach (var file in Directory.GetFiles(SessionsDir, "*.json"))
            {
                try
                {
                    using var stream = new FileStream(file,
                        FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                    var session = JsonSerializer.Deserialize<SessionInfo>(stream);
                    if (session != null && IsProcessAlive(session.Pid))
                    {
                        sessions.Add(session);
                    }
                }
                catch { }
            }

            ActiveSessions = sessions;
            DataChanged?.Invoke();
        }
        catch { }
    }

    private static bool IsProcessAlive(int pid)
    {
        try
        {
            var proc = Process.GetProcessById(pid);
            return !proc.HasExited;
        }
        catch
        {
            return false;
        }
    }

    public static string FormatTokens(long tokens)
    {
        return tokens switch
        {
            < 1_000 => tokens.ToString(),
            < 1_000_000 => $"{tokens / 1_000.0:F1}K",
            _ => $"{tokens / 1_000_000.0:F1}M"
        };
    }

    public static string FormatNumber(int n)
    {
        return n.ToString("N0");
    }
}
