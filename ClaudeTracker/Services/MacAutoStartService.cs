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
            var exePath = Environment.ProcessPath ?? "";
            var plist = $"""
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
}
