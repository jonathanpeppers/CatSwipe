#!/bin/bash

# CatSwipe Screenshot Capture Script
# This script automates the process of building the app, setting up an emulator,
# and capturing screenshots as requested in the issue.

set -e

echo "CatSwipe Screenshot Capture Script"
echo "=================================="
echo ""

# Configuration
EMULATOR_IMAGE="system-images;android-34;google_apis;x86_64"
EMULATOR_NAME="CatSwipeScreenshots"
APK_PACKAGE="com.companyname.catswipe"
SCREENSHOTS_DIR="docs/images"

# Function to check if a command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Check prerequisites
echo "Checking prerequisites..."

if ! command_exists dotnet; then
    echo "❌ .NET SDK not found. Please install .NET 9 SDK."
    exit 1
fi

if ! command_exists npm; then
    echo "❌ npm not found. Please install Node.js and npm."
    exit 1
fi

echo "✅ Prerequisites check passed"
echo ""

# Restore tools
echo "Restoring .NET tools..."
dotnet tool restore

# Install MAUI workload if not already installed
echo "Installing MAUI workload..."
dotnet workload install maui --version 9.0.100

# Build the Android app
echo "Building CatSwipe Android app..."
dotnet build CatSwipe/CatSwipe.csproj -f net9.0-android --configuration Release

# Find the built APK
APK_PATH=$(find CatSwipe/bin/Release/net9.0-android -name "*-Signed.apk" | head -1)
if [ -z "$APK_PATH" ]; then
    echo "❌ No signed APK found. Build may have failed."
    exit 1
fi

echo "✅ APK built successfully: $APK_PATH"
echo ""

# Set up Android emulator
echo "Setting up Android emulator..."

# Install platform tools
dotnet android sdk install --package platform-tools

# Install emulator
dotnet android sdk install --package emulator

# Install system image
dotnet android sdk install --package "$EMULATOR_IMAGE"

# Create AVD
dotnet android avd create --name "$EMULATOR_NAME" --sdk "$EMULATOR_IMAGE" --force

# Start emulator
echo "Starting Android emulator..."
dotnet android avd start --name "$EMULATOR_NAME" --gpu swiftshader_indirect --wait-boot --no-window --no-snapshot --no-audio --no-boot-anim

# Verify device is connected
dotnet android device list

echo "✅ Emulator started successfully"
echo ""

# Install the app
echo "Installing CatSwipe app on emulator..."
dotnet android device install --package "$APK_PATH"

echo "✅ App installed successfully"
echo ""

# Install and start Appium
echo "Setting up Appium..."
npm install -g appium
appium driver install uiautomator2

# Start Appium server in background
echo "Starting Appium server..."
appium server --log-level info &
APPIUM_PID=$!

# Wait for Appium to start
sleep 5

echo "✅ Appium server started (PID: $APPIUM_PID)"
echo ""

# Create screenshots directory
mkdir -p "$SCREENSHOTS_DIR"

# Run the screenshot capture test
echo "Capturing screenshots..."
dotnet test CatSwipe.UITests/CatSwipe.UITests.csproj --filter "FullyQualifiedName~ScreenshotCaptureTests" --configuration Release

# Cleanup
echo ""
echo "Cleaning up..."

# Stop Appium server
if [ ! -z "$APPIUM_PID" ]; then
    kill $APPIUM_PID 2>/dev/null || true
fi

# Stop emulator
dotnet android avd stop --name "$EMULATOR_NAME" || true

echo "✅ Cleanup completed"
echo ""

# Show results
echo "Screenshot capture completed!"
echo "Files created in $SCREENSHOTS_DIR:"
ls -la "$SCREENSHOTS_DIR"

echo ""
echo "Next steps:"
echo "1. Review the screenshots in $SCREENSHOTS_DIR"
echo "2. Commit the screenshots to git:"
echo "   git add $SCREENSHOTS_DIR"
echo "   git commit -m 'Add CatSwipe app screenshots'"
echo "   git push"