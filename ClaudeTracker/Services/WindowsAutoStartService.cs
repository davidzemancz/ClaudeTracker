using Microsoft.Win32;

namespace ClaudeTracker.Services;

public class WindowsAutoStartService : IAutoStartService
{
    private const string AutoStartKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "ClaudeTracker";

    public bool IsEnabled()
    {
        if (!OperatingSystem.IsWindows()) return false;
        using var key = Registry.CurrentUser.OpenSubKey(AutoStartKey, false);
        return key?.GetValue(AppName) != null;
    }

    public void SetEnabled(bool enable)
    {
        if (!OperatingSystem.IsWindows()) return;
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
