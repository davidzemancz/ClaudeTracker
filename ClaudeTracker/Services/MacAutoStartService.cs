using System.IO;

namespace ClaudeTracker.Services;

public class MacAutoStartService : IAutoStartService
{
    private static readonly string PlistPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "Library", "LaunchAgents", "com.claudetracker.plist");

    public bool IsEnabled()
    {
        return File.Exists(PlistPath);
    }

    public void SetEnabled(bool enable)
    {
        if (enable)
        {
            var appBundlePath = GetAppBundlePath();
            string plist;

            if (appBundlePath != null)
            {
                // Launch via 'open' so macOS uses the .app bundle (proper icon, Info.plist)
                plist = $"""
                    <?xml version="1.0" encoding="UTF-8"?>
                    <!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
                    <plist version="1.0">
                    <dict>
                        <key>Label</key>
                        <string>com.claudetracker</string>
                        <key>ProgramArguments</key>
                        <array>
                            <string>/usr/bin/open</string>
                            <string>-a</string>
                            <string>{appBundlePath}</string>
                        </array>
                        <key>RunAtLoad</key>
                        <true/>
                    </dict>
                    </plist>
                    """;
            }
            else
            {
                // Fallback: launch the binary directly
                var exePath = Environment.ProcessPath ?? "";
                plist = $"""
                    <?xml version="1.0" encoding="UTF-8"?>
                    <!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
                    <plist version="1.0">
                    <dict>
                        <key>Label</key>
                        <string>com.claudetracker</string>
                        <key>ProgramArguments</key>
                        <array>
                            <string>{exePath}</string>
                        </array>
                        <key>RunAtLoad</key>
                        <true/>
                    </dict>
                    </plist>
                    """;
            }

            var dir = Path.GetDirectoryName(PlistPath)!;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(PlistPath, plist);
        }
        else
        {
            if (File.Exists(PlistPath))
                File.Delete(PlistPath);
        }
    }

    /// <summary>
    /// Walks up from the current process path to find the enclosing .app bundle.
    /// Returns null if not running from a .app bundle.
    /// </summary>
    private static string? GetAppBundlePath()
    {
        var exePath = Environment.ProcessPath;
        if (string.IsNullOrEmpty(exePath)) return null;

        // Typical .app layout: Foo.app/Contents/MacOS/Foo
        var dir = Path.GetDirectoryName(exePath);
        while (dir != null)
        {
            if (dir.EndsWith(".app", StringComparison.OrdinalIgnoreCase))
                return dir;
            dir = Path.GetDirectoryName(dir);
        }

        // Also check /Applications for a known bundle
        const string appPath = "/Applications/Claude Tracker.app";
        if (Directory.Exists(appPath))
            return appPath;

        return null;
    }
}
