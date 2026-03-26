---
name: setup
description: Build, publish and install ClaudeTracker - a Windows system tray widget for monitoring Claude Code rate limits.
disable-model-invocation: true
---

# Setup ClaudeTracker

Follow these steps to build and install ClaudeTracker on the user's machine.

## 1. Check Prerequisites

Verify that .NET 10 SDK is installed:
```bash
dotnet --version
```
If not installed, tell the user to install .NET 10 SDK from https://dotnet.microsoft.com/download

## 2. Build

Publish as a self-contained single-file executable:
```bash
dotnet publish ClaudeTracker/ClaudeTracker.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ClaudeTracker/publish
```

## 3. Install

Copy the exe to a permanent location and enable autostart:
```bash
mkdir -p "$LOCALAPPDATA/ClaudeTracker"
cp ClaudeTracker/publish/ClaudeTracker.exe "$LOCALAPPDATA/ClaudeTracker/ClaudeTracker.exe"
```

Then add it to Windows startup via registry:
```bash
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v ClaudeTracker /t REG_SZ /d "\"$LOCALAPPDATA\\ClaudeTracker\\ClaudeTracker.exe\"" /f
```

## 4. Launch

Start the app:
```bash
"$LOCALAPPDATA/ClaudeTracker/ClaudeTracker.exe" &
```

## 5. Confirm

Tell the user:
- ClaudeTracker is now running in the system tray
- It shows Claude Code rate limit usage (5-hour and 7-day windows)
- Click the tray icon for details, use "Float" for an always-on-top mini window
- It will start automatically on Windows login
- The exe is installed at %LOCALAPPDATA%\ClaudeTracker\ClaudeTracker.exe

## Important

- The app reads OAuth credentials from `~/.claude/.credentials.json` - the user must be logged into Claude Code
- Rate limit data is fetched every 2.5 minutes from the Anthropic API
- If the user sees "Rate limited" error, this is normal - data will refresh on next poll
