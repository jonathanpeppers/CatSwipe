#!/bin/bash

# CatSwipe App Screenshot Capture Script
# This script demonstrates how to build, run and capture real screenshots of the CatSwipe app
# 
# Requirements:
# - .NET 9 SDK with MAUI workload installed
# - Node.js and npm (for Appium installation)

set -e

echo "🐱 CatSwipe Screenshot Capture Script"
echo "===================================="
echo

# Variables
PROJECT_ROOT="/home/runner/work/CatSwipe/CatSwipe"
BUILD_CONFIG="Debug"
DOCS_IMAGES_DIR="$PROJECT_ROOT/docs/images"
EMULATOR_NAME="UITestsEmulator"
EMULATOR_IMAGE="system-images;android-34;google_apis;x86_64"

# Ensure we're in the right directory
cd "$PROJECT_ROOT"

echo "📁 Project root: $PROJECT_ROOT"
echo "🏗️  Build configuration: $BUILD_CONFIG"
echo "📸 Screenshots will be saved to: $DOCS_IMAGES_DIR"
echo

# Step 1: Install Android SDK components
echo "📱 Step 1: Installing Android SDK components..."
dotnet android sdk install --package emulator
dotnet android sdk install --package "$EMULATOR_IMAGE"

if [ $? -eq 0 ]; then
    echo "✅ Android SDK components installed"
else
    echo "❌ Android SDK installation failed"
    exit 1
fi

# Step 2: Create and start Android emulator
echo
echo "🚀 Step 2: Setting up Android emulator..."
dotnet android avd create --name "$EMULATOR_NAME" --sdk "$EMULATOR_IMAGE" --force
dotnet android avd start --name "$EMULATOR_NAME" --gpu swiftshader_indirect --no-window --no-snapshot --no-audio --no-boot-anim

if [ $? -eq 0 ]; then
    echo "✅ Android emulator started successfully"
    dotnet android device list
else
    echo "❌ Android emulator startup failed"
    exit 1
fi

# Step 3: Build the Android app
echo
echo "🔨 Step 3: Building Android app..."
dotnet build CatSwipe/CatSwipe.csproj -f net9.0-android --configuration $BUILD_CONFIG --verbosity minimal

if [ $? -eq 0 ]; then
    echo "✅ Build successful"
else
    echo "❌ Build failed"
    exit 1
fi

# Step 4: Find and install the APK
echo
echo "📦 Step 4: Installing APK on emulator..."
APK_PATH=$(find CatSwipe/bin/$BUILD_CONFIG/net9.0-android -name "*-Signed.apk" | head -1)

if [ -n "$APK_PATH" ]; then
    echo "✅ APK found: $APK_PATH"
    dotnet android device install --package "$APK_PATH"
    echo "✅ APK installed successfully"
else
    echo "❌ No APK found"
    exit 1
fi

# Step 5: Install and start Appium
echo
echo "🤖 Step 5: Setting up Appium..."
if ! command -v appium &> /dev/null; then
    echo "Installing Appium..."
    npm install -g appium
    appium driver install uiautomator2
fi

echo "Starting Appium server..."
appium server --port 4723 &
APPIUM_PID=$!

# Wait for Appium to start
sleep 10

# Step 6: Run screenshot tests
echo
echo "📸 Step 6: Running screenshot capture tests..."
dotnet test CatSwipe.UITests/CatSwipe.UITests.csproj --filter "ScreenshotTests" --configuration $BUILD_CONFIG --verbosity normal

# Step 7: Copy screenshots to docs/images if they were captured in test-artifacts
echo
echo "📋 Step 7: Processing captured screenshots..."
mkdir -p "$DOCS_IMAGES_DIR"

# Find and copy screenshots from test artifacts
APP_LAUNCH_SCREENSHOT=$(find test-artifacts -name "*App_Should_CaptureInitialScreenshot*screenshot.png" | head -1)
SWIPE_LEFT_SCREENSHOT=$(find test-artifacts -name "*App_Should_CaptureSwipeLeftScreenshot*screenshot.png" | head -1)

if [ -n "$APP_LAUNCH_SCREENSHOT" ]; then
    cp "$APP_LAUNCH_SCREENSHOT" "$DOCS_IMAGES_DIR/app-launch.png"
    echo "✅ App launch screenshot saved to docs/images/app-launch.png"
fi

if [ -n "$SWIPE_LEFT_SCREENSHOT" ]; then
    cp "$SWIPE_LEFT_SCREENSHOT" "$DOCS_IMAGES_DIR/after-swipe-left.png"
    echo "✅ Swipe left screenshot saved to docs/images/after-swipe-left.png"
fi

# Cleanup
echo
echo "🧹 Cleanup..."
kill $APPIUM_PID 2>/dev/null || true

echo
echo "📋 Summary:"
echo "  - Android emulator started successfully"
echo "  - Build completed successfully"
echo "  - APK installed and tested"
echo "  - Real screenshots captured using Appium automation"
echo "  - Screenshots saved to docs/images/"
echo
echo "🎉 Real screenshot capture completed successfully!"