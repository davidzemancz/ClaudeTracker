---
name: setup
description: Build, publish and install ClaudeTracker - a cross-platform system tray widget for monitoring Claude Code rate limits (Windows and macOS).
disable-model-invocation: true
---

# Setup ClaudeTracker

Follow these steps to build and install ClaudeTracker on the user's machine. Detect the OS and follow the appropriate platform section.

## 1. Check Prerequisites

Verify that .NET 10 SDK is installed:
```bash
dotnet --version
```
If not installed, tell the user to install .NET 10 SDK from https://dotnet.microsoft.com/download

## 2. Build

### Windows
```bash
dotnet publish ClaudeTracker/ClaudeTracker.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ClaudeTracker/publish
```

### macOS (Apple Silicon)
```bash
dotnet publish ClaudeTracker/ClaudeTracker.csproj -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -o ClaudeTracker/publish
```

### macOS (Intel)
```bash
dotnet publish ClaudeTracker/ClaudeTracker.csproj -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -o ClaudeTracker/publish
```

## 3. Install

### Windows

Copy the exe to a permanent location:
```bash
mkdir -p "$LOCALAPPDATA/ClaudeTracker"
cp ClaudeTracker/publish/ClaudeTracker.exe "$LOCALAPPDATA/ClaudeTracker/ClaudeTracker.exe"
```

Then add it to Windows startup via registry:
```bash
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v ClaudeTracker /t REG_SZ /d "\"$LOCALAPPDATA\\ClaudeTracker\\ClaudeTracker.exe\"" /f
```

### macOS

Copy the binary to a permanent location:
```bash
mkdir -p "$HOME/.local/bin"
cp ClaudeTracker/publish/ClaudeTracker "$HOME/.local/bin/ClaudeTracker"
chmod +x "$HOME/.local/bin/ClaudeTracker"
```

The app will automatically create a LaunchAgent plist for autostart on first run.

## 4. Launch

### Windows
```bash
"$LOCALAPPDATA/ClaudeTracker/ClaudeTracker.exe" &
```

### macOS
```bash
"$HOME/.local/bin/ClaudeTracker" &
```

## 5. Confirm

Tell the user:
- ClaudeTracker is now running in the system tray (Windows) or menu bar (macOS)
- It shows Claude Code rate limit usage (5-hour and 7-day windows)
- On Windows: click the tray icon for details, use "Float" for an always-on-top mini window
- On macOS: right-click the menu bar icon for stats, use "Float" for an always-on-top mini window
- It will start automatically on login
- On Windows the exe is at %LOCALAPPDATA%\ClaudeTracker\ClaudeTracker.exe
- On macOS the binary is at ~/.local/bin/ClaudeTracker

## Important

- The app reads OAuth credentials from `~/.claude/.credentials.json` - the user must be logged into Claude Code
- Rate limit data is fetched every 2.5 minutes from the Anthropic API
- If the user sees "Rate limited" error, this is normal - data will refresh on next poll
