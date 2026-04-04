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

            // If token is expired, skip this cycle — let Claude Code handle refresh
            var expiresAt = DateTimeOffset.FromUnixTimeMilliseconds(oauth.ExpiresAt).UtcDateTime;
            if (DateTime.UtcNow >= expiresAt)
            {
                LastError = "Token expired (waiting for Claude Code to refresh)";
                UsageUpdated?.Invoke();
                return;
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

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
