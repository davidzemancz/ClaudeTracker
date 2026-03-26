using System.Text.Json.Serialization;

namespace ClaudeTracker.Models;

public class UsageResponse
{
    [JsonPropertyName("five_hour")]
    public UsageWindow? FiveHour { get; set; }

    [JsonPropertyName("seven_day")]
    public UsageWindow? SevenDay { get; set; }
}

public class UsageWindow
{
    [JsonPropertyName("utilization")]
    public double Utilization { get; set; }

    [JsonPropertyName("resets_at")]
    public DateTime ResetsAt { get; set; }
}
