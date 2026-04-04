using System.Text.Json.Serialization;

namespace ClaudeTracker.Models;

public class CredentialsFile
{
    [JsonPropertyName("claudeAiOauth")]
    public OAuthCredentials? ClaudeAiOauth { get; set; }
}

public class OAuthCredentials
{
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; } = "";

    [JsonPropertyName("refreshToken")]
    public string RefreshToken { get; set; } = "";

    [JsonPropertyName("expiresAt")]
    public long ExpiresAt { get; set; }

    [JsonPropertyName("subscriptionType")]
    public string SubscriptionType { get; set; } = "";

    [JsonPropertyName("rateLimitTier")]
    public string RateLimitTier { get; set; } = "";
}