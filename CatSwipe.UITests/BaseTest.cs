using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;

namespace CatSwipe.UITests;

public abstract class BaseTest : IDisposable
{
    protected AndroidDriver Driver { get; private set; } = null!;

    public string PackageName { get; } = "com.companyname.catswipe";
    public string ActivityName { get; } = "MainActivity";

    private static readonly string ArtifactsPath = GetArtifactsPath();

    private static string GetArtifactsPath()
    {
        string? assemblyLocation = Assembly.GetExecutingAssembly().Location;
        if (string.IsNullOrEmpty(assemblyLocation))
        {
            Console.WriteLine("Warning: Assembly location is null or empty, falling back to current directory");
            return Path.Combine(Environment.CurrentDirectory, "test-artifacts");
        }

        string? assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
        if (string.IsNullOrEmpty(assemblyDirectory))
        {
            Console.WriteLine("Warning: Assembly directory is null or empty, falling back to current directory");
            return Path.Combine(Environment.CurrentDirectory, "test-artifacts");
        }

        // Navigate up from bin/Debug/net9.0/ to project root, then create test-artifacts
        string artifactsPath = Path.Combine(assemblyDirectory, "..", "..", "..", "..", "test-artifacts");
        string fullPath = Path.GetFullPath(artifactsPath);

        Console.WriteLine($"Assembly location: {assemblyLocation}");
        Console.WriteLine($"Assembly directory: {assemblyDirectory}");
        Console.WriteLine($"Artifacts path: {fullPath}");

        return fullPath;
    }

    private static string FindApkPath()
    {
        // Start from the current working directory and look for the APK
        var currentDir = Environment.CurrentDirectory;
        Console.WriteLine($"Looking for APK from current directory: {currentDir}");

        // Go up to find the repository root (look for .git folder or specific project structure)
        var repoRoot = FindRepositoryRoot(currentDir);
        Console.WriteLine($"Repository root: {repoRoot}");

        if (!string.IsNullOrEmpty(repoRoot))
        {
            var apkPath = Path.Combine(repoRoot, "CatSwipe", "bin", "Debug", "net9.0-android", "com.companyname.catswipe-Signed.apk");
            if (File.Exists(apkPath))
            {
                var fullApkPath = Path.GetFullPath(apkPath);
                Console.WriteLine($"Found APK at: {fullApkPath}");
                return fullApkPath;
            }
        }

        // Fallback: Look for signed APK files recursively
        var apkPattern = "*-Signed.apk";
        var searchPaths = new[]
        {
            currentDir,
            Path.Combine(currentDir, "CatSwipe", "bin", "Debug", "net9.0-android"),
            Path.Combine(currentDir, "..", "CatSwipe", "bin", "Debug", "net9.0-android"),
            Path.Combine(currentDir, "..", "..", "CatSwipe", "bin", "Debug", "net9.0-android"),
            Path.Combine(currentDir, "..", "..", "..", "CatSwipe", "bin", "Debug", "net9.0-android"),
            Path.Combine(currentDir, "..", "..", "..", "..", "CatSwipe", "bin", "Debug", "net9.0-android")
        };

        foreach (var searchPath in searchPaths)
        {
            if (Directory.Exists(searchPath))
            {
                var apkFiles = Directory.GetFiles(searchPath, apkPattern, SearchOption.TopDirectoryOnly);
                if (apkFiles.Length > 0)
                {
                    var apkPath = Path.GetFullPath(apkFiles[0]);
                    Console.WriteLine($"Found APK at: {apkPath}");
                    return apkPath;
                }
            }
        }

        // Last resort: search recursively from current directory up to 4 levels
        for (int levels = 0; levels < 4; levels++)
        {
            var searchRoot = currentDir;
            for (int i = 0; i < levels; i++)
            {
                searchRoot = Path.GetDirectoryName(searchRoot) ?? searchRoot;
            }

            if (Directory.Exists(searchRoot))
            {
                var apkFiles = Directory.GetFiles(searchRoot, apkPattern, SearchOption.AllDirectories);
                if (apkFiles.Length > 0)
                {
                    var apkPath = Path.GetFullPath(apkFiles[0]);
                    Console.WriteLine($"Found APK at: {apkPath}");
                    return apkPath;
                }
            }
        }

        Console.WriteLine("APK not found in expected locations");
        return "";
    }

    private static string FindRepositoryRoot(string startPath)
    {
        var current = startPath;
        while (!string.IsNullOrEmpty(current))
        {
            if (Directory.Exists(Path.Combine(current, ".git")) || 
                File.Exists(Path.Combine(current, "cat-swipe.sln")))
            {
                return current;
            }
            current = Path.GetDirectoryName(current);
        }
        return "";
    }

    static BaseTest()
    {
        // Ensure artifacts directory exists
        Directory.CreateDirectory(ArtifactsPath);
        Console.WriteLine($"Created artifacts directory: {ArtifactsPath}");
    }

    protected void InitializeAndroidDriver()
    {
        // Find the APK path dynamically
        var apkPath = FindApkPath();
        if (string.IsNullOrEmpty(apkPath))
        {
            throw new FileNotFoundException("Could not find the signed APK file");
        }

        Console.WriteLine($"Using APK: {apkPath}");

        // Create Android-specific options - use a more basic approach
        var options = new AppiumOptions();
        
        // Set basic capabilities
        options.AddAdditionalAppiumOption("platformName", "Android");
        options.AutomationName = "UiAutomator2";
        options.AddAdditionalAppiumOption("appium:newCommandTimeout", 300);
        options.AddAdditionalAppiumOption("appium:connectHardwareKeyboard", true);
        
        // Don't try to auto-launch the app, just connect to the device
        options.AddAdditionalAppiumOption("appium:noReset", true);
        options.AddAdditionalAppiumOption("appium:dontStopAppOnReset", true);

        // Create driver with default Appium server URL
        var serverUri = new Uri("http://127.0.0.1:4723");
        Driver = new AndroidDriver(serverUri, options);

        // Configure implicit wait timeout
        // Reference: http://appium.io/docs/en/latest/quickstart/test-dotnet/
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        
        // Manually start the app using adb-like commands through Appium
        try
        {
            Console.WriteLine("Attempting to launch CatSwipe app manually...");
            Driver.StartActivity(PackageName, ActivityName);
            Console.WriteLine("App launched successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Direct activity launch failed: {ex.Message}");
            // Try alternative approach - activate the app if it's installed
            try
            {
                Driver.ActivateApp(PackageName);
                Console.WriteLine("App activated successfully");
            }
            catch (Exception ex2)
            {
                Console.WriteLine($"App activation also failed: {ex2.Message}");
                // As last resort, just continue - maybe the app is already running
            }
        }
    }

    protected void CaptureTestFailureDiagnostics([CallerMemberName] string testName = "")
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var testArtifactDir = Path.Combine(ArtifactsPath, $"{testName}-{timestamp}");
        Directory.CreateDirectory(testArtifactDir);

        try
        {
            // Capture screenshot
            CaptureScreenshot(testArtifactDir, testName);

            // Capture logcat output
            CaptureLogcat(testArtifactDir, testName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to capture diagnostics for test {testName}: {ex.Message}");
        }
    }

    private void CaptureScreenshot(string artifactDir, string testName)
    {
        try
        {
            if (Driver?.SessionId != null)
            {
                var screenshot = Driver.GetScreenshot();
                var screenshotPath = Path.Combine(artifactDir, $"{testName}-screenshot.png");
                screenshot.SaveAsFile(screenshotPath);
                Console.WriteLine($"Screenshot saved: {screenshotPath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to capture screenshot: {ex.Message}");
        }
    }

    private void CaptureLogcat(string artifactDir, string testName)
    {
        try
        {
            var logcatPath = Path.Combine(artifactDir, $"{testName}-logcat.txt");

            // Run adb logcat command to capture recent logs
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "adb",
                Arguments = "logcat -d -v time",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processStartInfo);
            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    File.WriteAllText(logcatPath, output);
                    Console.WriteLine($"Logcat saved: {logcatPath}");
                }
                else
                {
                    Console.WriteLine($"adb logcat failed with exit code {process.ExitCode}: {error}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to capture logcat: {ex.Message}");
        }
    }

    public void Dispose()
    {
        Driver?.Quit();
    }
}
