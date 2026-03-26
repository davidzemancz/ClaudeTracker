#!/bin/bash
# Creates a macOS .app bundle for Claude Tracker and installs it to /Applications
set -e

APP_NAME="Claude Tracker"
BUNDLE_DIR="$APP_NAME.app"
PUBLISH_DIR="ClaudeTracker/publish"
INSTALL_DIR="/Applications/$BUNDLE_DIR"

# Always publish fresh
echo "Publishing ClaudeTracker..."
dotnet publish ClaudeTracker/ClaudeTracker.csproj -c Release -r osx-arm64 --self-contained -o "$PUBLISH_DIR"

# Create .app bundle structure
rm -rf "$BUNDLE_DIR"
mkdir -p "$BUNDLE_DIR/Contents/MacOS"
mkdir -p "$BUNDLE_DIR/Contents/Resources"

# Copy Info.plist
cp ClaudeTracker/Info.plist "$BUNDLE_DIR/Contents/"

# Copy icon
cp ClaudeTracker/Assets/icon.icns "$BUNDLE_DIR/Contents/Resources/icon.icns"

# Copy all published files
cp -R "$PUBLISH_DIR"/* "$BUNDLE_DIR/Contents/MacOS/"

# Make executable
chmod +x "$BUNDLE_DIR/Contents/MacOS/ClaudeTracker"

echo "Created $BUNDLE_DIR"

# Install to /Applications
echo "Installing to /Applications..."
rm -rf "$INSTALL_DIR"
cp -R "$BUNDLE_DIR" "$INSTALL_DIR"
echo "Installed to $INSTALL_DIR"

# Clean up local bundle
rm -rf "$BUNDLE_DIR"

echo ""
echo "Done! Launch with: open '/Applications/$BUNDLE_DIR'"
