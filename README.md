# ClaudeTracker

Cross-platform system tray widget for monitoring your Claude Code rate limit usage. Built with Avalonia UI.

![.NET 10](https://img.shields.io/badge/.NET-10-blue)
![Avalonia UI](https://img.shields.io/badge/Avalonia-11.3-blue)
![Windows](https://img.shields.io/badge/platform-Windows-lightgrey)
![macOS](https://img.shields.io/badge/platform-macOS-lightgrey)

## What it does

- Shows your **5-hour** and **7-day** rate limit usage as a percentage in the system tray
- Tray icon is color-coded: **green** (<50%), **yellow** (50-80%), **red** (>80%)
- Click the tray icon for a popup with progress bars and reset times
- Optional **floating always-on-top** mini window
- Tracks active Claude Code sessions
- **Auto-starts** by default (Windows Registry / macOS LaunchAgent)

## Quick Setup (with Claude Code)

```bash
git clone https://github.com/davidzemancz/ClaudeTracker.git
cd ClaudeTracker
claude
```

Then type `/setup` and Claude will build, install and launch it for you.

## Manual Setup

### Prerequisites

- Windows 10/11 or macOS
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Active Claude Code login

### Build & Run

**Windows:**

```bash
dotnet publish ClaudeTracker/ClaudeTracker.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ClaudeTracker/publish
```

Then run `ClaudeTracker/publish/ClaudeTracker.exe`.

**macOS:**

```bash
dotnet publish ClaudeTracker/ClaudeTracker.csproj -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ClaudeTracker/publish
```

Then run `ClaudeTracker/publish/ClaudeTracker`.

### Install permanently

**Windows:** Copy the exe to `%LOCALAPPDATA%\ClaudeTracker\`.

**macOS:** Copy the binary to `~/.local/bin/` or another location on your PATH.

The app enables autostart on first launch. You can toggle this in the tray popup.

## How it works

- Reads OAuth credentials from macOS Keychain (`Claude Code-credentials`) or `~/.claude/.credentials.json` as fallback
- Polls `GET https://api.anthropic.com/api/oauth/usage` every 2.5 minutes
- Monitors `~/.claude/sessions/` for active Claude Code sessions
- Handles token refresh automatically

## License

MIT
