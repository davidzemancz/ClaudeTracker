# ClaudeTracker

Windows system tray widget for monitoring your Claude Code rate limit usage.

![.NET 10](https://img.shields.io/badge/.NET-10-blue)
![Windows](https://img.shields.io/badge/platform-Windows-lightgrey)

## What it does

- Shows your **5-hour** and **7-day** rate limit usage as a percentage in the system tray
- Tray icon is color-coded: **green** (<50%), **yellow** (50-80%), **red** (>80%)
- Click the tray icon for a popup with progress bars and reset times
- Optional **floating always-on-top** mini window
- Tracks active Claude Code sessions
- **Auto-starts with Windows** by default

## Quick Setup (with Claude Code)

```bash
git clone https://github.com/davidzemancz/ClaudeTracker.git
cd ClaudeTracker
claude
```

Then type `/setup` and Claude will build, install and launch it for you.

## Manual Setup

### Prerequisites

- Windows 10/11
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Active Claude Code login (`~/.claude/.credentials.json` must exist)

### Build & Run

```bash
dotnet publish ClaudeTracker/ClaudeTracker.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ClaudeTracker/publish
```

Then run `ClaudeTracker/publish/ClaudeTracker.exe`.

### Install permanently

Copy the exe to a permanent location:

```bash
mkdir "%LOCALAPPDATA%\ClaudeTracker"
copy ClaudeTracker\publish\ClaudeTracker.exe "%LOCALAPPDATA%\ClaudeTracker\"
```

The app enables Windows autostart on first launch. You can toggle this in the tray popup.

## How it works

- Reads OAuth credentials from `~/.claude/.credentials.json`
- Polls `GET https://api.anthropic.com/api/oauth/usage` every 2.5 minutes
- Monitors `~/.claude/sessions/` for active Claude Code sessions
- Handles token refresh automatically

## License

MIT
