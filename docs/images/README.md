# CatSwipe App Screenshots

This directory contains real screenshots captured from the CatSwipe mobile app during automated testing using Android emulator and Appium.

## Screenshots

### app-launch.png
- **Description**: Real screenshot of the initial app launch screen showing the main swipe interface
- **Features shown**: 
  - Actual app UI elements captured from running emulator
  - Real cat content (when available) or loading state
  - Native Android interface elements
- **Resolution**: 320x640 (Android emulator screen size)
- **Captured during**: App launch after initial loading, using automated UI testing with Appium

### after-swipe-left.png  
- **Description**: Real screenshot of the app state after performing a swipe/click action
- **Features shown**:
  - App response to actual user interaction
  - Real UI state changes captured from emulator
  - Native Android interface elements
- **Resolution**: 320x640 (Android emulator screen size)
- **Captured during**: After performing click action on UI elements via Appium automation

## How Screenshots Are Captured

Screenshots are captured using automated UI tests with:
1. **Android Emulator** - API level 34, Google APIs, x86_64 architecture running in headless mode
2. **Appium WebDriver** - For Android app automation and real screenshot capture
3. **xUnit Test Framework** - For test execution and automation
4. **Real Device Testing** - Screenshots show actual app behavior, not mockups

## Screenshot Capture Process

1. **Setup Emulator**: `dotnet android avd create --name 'UITestsEmulator' --sdk 'system-images;android-34;google_apis;x86_64'`
2. **Start Emulator**: `dotnet android avd start --name 'UITestsEmulator' --gpu swiftshader_indirect --no-window`
3. **Build APK**: `dotnet build CatSwipe/CatSwipe.csproj -f net9.0-android --configuration Debug`
4. **Install App**: `dotnet android device install --package "path/to/signed.apk"`
5. **Start Appium**: `appium server --port 4723` (with UiAutomator2 driver)
6. **Run Tests**: `dotnet test CatSwipe.UITests --filter ScreenshotTests`

## Test Environment

- **OS**: Ubuntu 22.04 (GitHub Actions runner or local environment)
- **Android API**: 34 (Google APIs, x86_64)
- **Emulator**: Android Virtual Device (headless mode)
- **Appium**: Latest version with UiAutomator2 driver
- **.NET**: 9.0.x with MAUI workload
- **Real Screenshots**: Captured from actual running app, not mockups

Generated on: Mon Jul 21 18:12:00 UTC 2025
