using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace CatSwipe.UITests;

/// <summary>
/// Screenshot capture tests for demonstrating app launch and swipe functionality.
/// These tests would normally run with Android emulator and Appium server.
/// In CI/CD, they demonstrate the complete UI testing workflow.
/// </summary>
public class ScreenshotTests : BaseTest
{
    private const int WaitForContentLoadMs = 3000;
    private const int SwipeWaitMs = 4000;

    private readonly string _screenshotDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "work", "CatSwipe", "CatSwipe", "docs", "images");

    [Fact] 
    public void App_Should_CaptureInitialScreenshot()
    {
        // This test captures the initial app state after launch
        try
        {
            Console.WriteLine("🚀 Starting app launch screenshot test...");
            
            InitializeAndroidDriver();
            
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));
            
            // Wait for the loading indicator to disappear
            WaitForLoadingToComplete(wait);
            
            // Wait for cat content to fully load
            Thread.Sleep(WaitForContentLoadMs);
            
            // Verify key UI elements are present
            VerifyMainUIElements(wait);
            
            // Capture initial screenshot
            CaptureScreenshot("app-launch");
            
            Console.WriteLine("✅ App launch screenshot captured successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ App launch screenshot test failed: {ex.Message}");
            CaptureTestFailureDiagnostics();
            throw;
        }
    }

    [Fact]
    public void App_Should_CaptureSwipeLeftScreenshot() 
    {
        // This test performs a swipe left action and captures the result
        try
        {
            Console.WriteLine("👈 Starting swipe left screenshot test...");
            
            InitializeAndroidDriver();
            
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));
            
            // Wait for initial content to load
            WaitForLoadingToComplete(wait);
            Thread.Sleep(WaitForContentLoadMs);
            
            // Perform swipe left action (using dislike button)
            PerformSwipeLeft(wait);
            
            // Wait for swipe animation and next cat to load
            Thread.Sleep(SwipeWaitMs);
            
            // Capture screenshot after swipe
            CaptureScreenshot("after-swipe-left");
            
            Console.WriteLine("✅ Swipe left screenshot captured successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Swipe left screenshot test failed: {ex.Message}");
            CaptureTestFailureDiagnostics();
            throw;
        }
    }

    private void WaitForLoadingToComplete(WebDriverWait wait)
    {
        // Wait for the loading indicator to disappear
        wait.Until(driver =>
        {
            try
            {
                var loadingElements = driver.FindElements(By.XPath("//*[contains(@text, 'Loading cats')]"));
                return loadingElements.All(element => !element.Displayed);
            }
            catch (NoSuchElementException)
            {
                return true; // Loading indicator not found, assuming loading is complete
            }
        });
    }

    private void VerifyMainUIElements(WebDriverWait wait)
    {
        // Verify that main UI elements are present
        var appTitle = wait.Until(driver =>
            driver.FindElement(By.XPath("//*[contains(@text, 'CatSwipe')]")));
        Assert.True(appTitle.Displayed, "App title should be visible");

        // Verify action buttons are present
        var dislikeButton = Driver.FindElement(By.XPath("//*[contains(@text, '❌')]"));
        var likeButton = Driver.FindElement(By.XPath("//*[contains(@text, '❤️')]"));
        
        Assert.True(dislikeButton.Displayed, "Dislike button should be visible");
        Assert.True(likeButton.Displayed, "Like button should be visible");
    }

    private void PerformSwipeLeft(WebDriverWait wait)
    {
        // Find and click the dislike button (equivalent to swiping left)
        var dislikeButton = wait.Until(driver =>
            driver.FindElement(By.XPath("//*[contains(@text, '❌')]")));
        
        Assert.True(dislikeButton.Displayed, "Dislike button should be available for interaction");
        
        dislikeButton.Click();
        Console.WriteLine("👈 Performed swipe left action (dislike button clicked)");
    }

    private void CaptureScreenshot(string testName)
    {
        try
        {
            // Ensure screenshot directory exists
            Directory.CreateDirectory(_screenshotDir);
            
            if (Driver?.SessionId != null)
            {
                var screenshot = Driver.GetScreenshot();
                var screenshotPath = Path.Combine(_screenshotDir, $"{testName}.png");
                screenshot.SaveAsFile(screenshotPath);
                Console.WriteLine($"📸 Screenshot saved: {screenshotPath}");
            }
            else
            {
                throw new InvalidOperationException("Driver session is not available for screenshot capture");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to capture screenshot: {ex.Message}");
            throw;
        }
    }
}