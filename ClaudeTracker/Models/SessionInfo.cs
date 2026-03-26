using System.Text.Json.Serialization;

namespace ClaudeTracker.Models;

public class SessionInfo
{
    [JsonPropertyName("pid")]
    public int Pid { get; set; }

    [JsonPropertyName("sessionId")]
    public string SessionId { get; set; } = "";

    [JsonPropertyName("cwd")]
    public string Cwd { get; set; } = "";

    [JsonPropertyName("startedAt")]
    public long StartedAt { get; set; }

    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "";

    [JsonPropertyName("entrypoint")]
    public string Entrypoint { get; set; } = "";
}
