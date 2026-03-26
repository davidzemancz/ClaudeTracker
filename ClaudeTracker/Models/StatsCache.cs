using System.Text.Json.Serialization;

namespace ClaudeTracker.Models;

public class StatsCache
{
    [JsonPropertyName("version")]
    public int Version { get; set; }

    [JsonPropertyName("lastComputedDate")]
    public string LastComputedDate { get; set; } = "";

    [JsonPropertyName("dailyActivity")]
    public List<DailyActivityEntry> DailyActivity { get; set; } = [];

    [JsonPropertyName("dailyModelTokens")]
    public List<DailyModelTokensEntry> DailyModelTokens { get; set; } = [];

    [JsonPropertyName("modelUsage")]
    public Dictionary<string, ModelUsageEntry> ModelUsage { get; set; } = [];

    [JsonPropertyName("totalSessions")]
    public int TotalSessions { get; set; }

    [JsonPropertyName("totalMessages")]
    public int TotalMessages { get; set; }

    [JsonPropertyName("longestSession")]
    public LongestSessionEntry? LongestSession { get; set; }

    [JsonPropertyName("firstSessionDate")]
    public string FirstSessionDate { get; set; } = "";

    [JsonPropertyName("hourCounts")]
    public Dictionary<string, int> HourCounts { get; set; } = [];
}

public class DailyActivityEntry
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = "";

    [JsonPropertyName("messageCount")]
    public int MessageCount { get; set; }

    [JsonPropertyName("sessionCount")]
    public int SessionCount { get; set; }

    [JsonPropertyName("toolCallCount")]
    public int ToolCallCount { get; set; }
}

public class DailyModelTokensEntry
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = "";

    [JsonPropertyName("tokensByModel")]
    public Dictionary<string, long> TokensByModel { get; set; } = [];
}

public class ModelUsageEntry
{
    [JsonPropertyName("inputTokens")]
    public long InputTokens { get; set; }

    [JsonPropertyName("outputTokens")]
    public long OutputTokens { get; set; }

    [JsonPropertyName("cacheReadInputTokens")]
    public long CacheReadInputTokens { get; set; }

    [JsonPropertyName("cacheCreationInputTokens")]
    public long CacheCreationInputTokens { get; set; }

    [JsonPropertyName("costUSD")]
    public double CostUSD { get; set; }
}

public class LongestSessionEntry
{
    [JsonPropertyName("sessionId")]
    public string SessionId { get; set; } = "";

    [JsonPropertyName("duration")]
    public long Duration { get; set; }

    [JsonPropertyName("messageCount")]
    public int MessageCount { get; set; }

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = "";
}
