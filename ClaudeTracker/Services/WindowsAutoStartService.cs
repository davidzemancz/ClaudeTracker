using Microsoft.Win32;

namespace ClaudeTracker.Services;

public class WindowsAutoStartService : IAutoStartService
{
    private const string AutoStartKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "ClaudeTracker";

    public bool IsEnabled()
    {
        if (!OperatingSystem.IsWindows()) return false;
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(AutoStartKey, false);
            return key?.GetValue(AppName) != null;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public void SetEnabled(bool enable)
    {
        if (!OperatingSystem.IsWindows()) return;
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(AutoStartKey, true);

            if (enable)
            {
                var exePath = Environment.ProcessPath;
                if (string.IsNullOrEmpty(exePath)) return;
                key.SetValue(AppName, $"\"{exePath}\"");
            }
            else
            {
                key.DeleteValue(AppName, false);
            }
        }
        catch (Exception)
        {
            // Registry access may fail due to permissions or policy
        }
    }
}
