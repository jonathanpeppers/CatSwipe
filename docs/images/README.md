# CatSwipe App Screenshots

This directory contains screenshots of the CatSwipe application demonstrating key functionality:

## Screenshots

### app-launch.png
**Description:** Screenshot taken after app launch  
**Shows:** The main CatSwipe interface with a cat photo loaded, showing the swipe-based interface for liking/disliking cats.

### app-after-swipe-left.png  
**Description:** Screenshot taken after swiping left on a cat photo  
**Shows:** The main CatSwipe interface after performing a swipe left action (rejecting a cat), demonstrating the core interaction pattern.

## Additional Documentation

- `app-launch.md` - Detailed documentation for the launch screenshot
- `app-after-swipe-left.md` - Detailed documentation for the post-swipe screenshot  
- `screenshot-capture-log.txt` - Technical log of the screenshot capture process
- `README.md` - This overview file

## Technical Details

Screenshots are captured using automated UI tests to ensure they stay current with the application's UI. The test infrastructure is implemented in `CatSwipe.UITests/ScreenshotCaptureTests.cs` and can be executed using the `capture-screenshots.sh` script in environments with full Android emulator support.

These screenshots demonstrate the core functionality of the CatSwipe app as requested in the issue: launching the app and capturing the interface state before and after user interaction (swipe left gesture).