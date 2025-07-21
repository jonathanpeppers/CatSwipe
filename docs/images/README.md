# CatSwipe App Screenshots

This directory contains screenshots captured from the CatSwipe mobile app during automated testing.

## Screenshots

### app-launch.png
- **Description**: Initial app launch screen showing the main swipe interface
- **Features shown**: 
  - App header with "🐱 CatSwipe" title
  - Heart button for accessing liked cats collection
  - Cat card with image and breed information
  - Like (❤️) and Dislike (❌) action buttons
- **Resolution**: 1080x2400 (Android phone)
- **Captured during**: App launch after initial loading completes

### after-swipe-left.png  
- **Description**: App state after swiping left (dislike action)
- **Features shown**:
  - Next cat card displayed
  - Same UI layout as initial screen
  - New cat image and breed information
  - Action buttons ready for next interaction
- **Resolution**: 1080x2400 (Android phone)
- **Captured during**: 4 seconds after dislike button interaction

## How Screenshots Are Captured

Screenshots are captured using automated UI tests with:
1. **Appium WebDriver** - For Android app automation
2. **Android Emulator** - Running API level 34
3. **xUnit Test Framework** - For test execution
4. **GitHub Actions CI/CD** - For automated testing

## Screenshot Capture Process

1. Build Android APK: `dotnet build CatSwipe/CatSwipe.csproj -f net9.0-android`
2. Start Android emulator: `dotnet android avd start --name UITestsEmulator`
3. Install app: `adb install CatSwipe-Signed.apk`
4. Start Appium server: `appium server --port 4723`
5. Run screenshot tests: `dotnet test CatSwipe.UITests --filter ScreenshotTests`

## Test Environment

- **OS**: Ubuntu 22.04 (GitHub Actions runner)
- **Android API**: 34 (Google APIs, x86_64)
- **Emulator**: Android Virtual Device
- **Appium**: Latest version with UiAutomator2 driver
- **.NET**: 9.0.x with MAUI workload

Generated on: Mon Jul 21 17:19:01 UTC 2025
