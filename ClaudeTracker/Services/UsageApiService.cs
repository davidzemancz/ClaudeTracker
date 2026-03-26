using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text.Json;
using ClaudeTracker.Models;

namespace ClaudeTracker.Services;

public class UsageApiService : IDisposable
{
    private static readonly string CredentialsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".claude", ".credentials.json");

    private const string UsageEndpoint = "https://api.anthropic.com/api/oauth/usage";
    private const string TokenEndpoint = "https://console.anthropic.com/v1/oauth/token";
    private const string ClientId = "9d1c250a-e61b-44d9-88ed-5944d1962f5e";

    private readonly HttpClient _httpClient = new();
    private UsageResponse? _cachedUsage;
    private DateTime _lastFetchTime = DateTime.MinValue;

    public event Action? UsageUpdated;

    public UsageResponse? CurrentUsage => _cachedUsage;
    public DateTime LastFetchTime => _lastFetchTime;
    public string? LastError { get; private set; }

    public async Task FetchUsageAsync()
    {
        try
        {
            var creds = await ReadCredentialsAsync();
            if (creds?.ClaudeAiOauth == null)
            {
                LastError = "No credentials found";
                return;
            }

            var oauth = creds.ClaudeAiOauth;

            // Check if token is expired
            var expiresAt = DateTimeOffset.FromUnixTimeMilliseconds(oauth.ExpiresAt).UtcDateTime;
            if (DateTime.UtcNow >= expiresAt)
            {
                var refreshed = await RefreshTokenAsync(oauth.RefreshToken);
                if (refreshed == null)
                {
                    LastError = "Token refresh failed";
                    return;
                }
                oauth.AccessToken = refreshed.AccessToken;
                oauth.RefreshToken = refreshed.RefreshToken;
                oauth.ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(refreshed.ExpiresIn).ToUnixTimeMilliseconds();
                await SaveCredentialsAsync(creds);
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, UsageEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", oauth.AccessToken);
            request.Headers.Add("anthropic-beta", "oauth-2025-04-20");

            using var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                LastError = "Rate limited (using cached data)";
                // Keep cached data, don't overwrite
                UsageUpdated?.Invoke();
                return;
            }

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            _cachedUsage = JsonSerializer.Deserialize<UsageResponse>(json);
            _lastFetchTime = DateTime.Now;
            LastError = null;
            UsageUpdated?.Invoke();
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            // Still notify so UI can show error state
            UsageUpdated?.Invoke();
        }
    }

    private async Task<TokenRefreshResponse?> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var payload = new
            {
                grant_type = "refresh_token",
                refresh_token = refreshToken,
                client_id = ClientId
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                System.Text.Encoding.UTF8,
                "application/json");

            using var response = await _httpClient.PostAsync(TokenEndpoint, content);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TokenRefreshResponse>(json);
        }
        catch
        {
            return null;
        }
    }

    private static async Task<CredentialsFile?> ReadCredentialsAsync()
    {
        // On macOS, try reading from Keychain first
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var keychainCreds = ReadFromKeychain();
            if (keychainCreds != null) return keychainCreds;
        }

        // Fall back to file-based credentials (Windows or if Keychain read fails)
        if (!File.Exists(CredentialsPath)) return null;

        await using var stream = new FileStream(CredentialsPath,
            FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        return await JsonSerializer.DeserializeAsync<CredentialsFile>(stream);
    }

    private static CredentialsFile? ReadFromKeychain()
    {
        try
        {
            var username = Environment.UserName;
            var psi = new ProcessStartInfo
            {
                FileName = "security",
                Arguments = $"find-generic-password -s \"Claude Code-credentials\" -a \"{username}\" -w",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null) return null;

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(5000);

            if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(output)) return null;

            return JsonSerializer.Deserialize<CredentialsFile>(output.Trim());
        }
        catch
        {
            return null;
        }
    }

    private static async Task SaveCredentialsAsync(CredentialsFile creds)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            SaveToKeychain(creds);
            return;
        }

        var json = JsonSerializer.Serialize(creds, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(CredentialsPath, json);
    }

    private static void SaveToKeychain(CredentialsFile creds)
    {
        try
        {
            var json = JsonSerializer.Serialize(creds);
            var username = Environment.UserName;

            // Delete existing entry first, then add updated one
            var deletePsi = new ProcessStartInfo
            {
                FileName = "security",
                Arguments = $"delete-generic-password -s \"Claude Code-credentials\" -a \"{username}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (var deleteProc = Process.Start(deletePsi))
                deleteProc?.WaitForExit(5000);

            var addPsi = new ProcessStartInfo
            {
                FileName = "security",
                Arguments = $"add-generic-password -s \"Claude Code-credentials\" -a \"{username}\" -w \"{json.Replace("\"", "\\\"")}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var addProc = Process.Start(addPsi);
            addProc?.WaitForExit(5000);
        }
        catch
        {
            // Silently fail - next fetch will re-read from Keychain
        }
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
