using Allure.Net.Commons;
using OpenQA.Selenium.Appium.Android;
using Reqnroll;
using Shoppix.MobileApp.Automation.Core;

namespace Shoppix.MobileApp.Automation.Support;

/// <summary>
/// Session and scenario lifecycle, equivalent to conftest.py:
/// session-scoped driver creation, .env + run profile loading, APK/activity
/// resolution and failure artifact capture (screenshot + page source).
/// </summary>
[Binding]
public sealed class Hooks
{
    private const string ProfileRelativePath = "Configs/Profiles/stage.android.local.json";
    private const string LoginDataRelativePath = "Data/Login/stage_login_data.json";

    [BeforeTestRun]
    public static void BeforeTestRun()
    {
        var root = FindProjectRoot();

        LoadEnvironment(root);
        TestHost.Email = Environment.GetEnvironmentVariable("SHOPPIX_LOGIN_EMAIL") ?? "";
        TestHost.Password = Environment.GetEnvironmentVariable("SHOPPIX_LOGIN_PASSWORD") ?? "";
        TestHost.GmailAddress = Environment.GetEnvironmentVariable("GMAIL_ADDRESS") ?? "";
        TestHost.GmailAppPassword = Environment.GetEnvironmentVariable("GMAIL_APP_PASSWORD") ?? "";

        var loginDataPath = Path.Combine(root, LoginDataRelativePath.Replace('/', Path.DirectorySeparatorChar));
        TestHost.LoginData = LoginData.Load(loginDataPath);

        var profile = BuildRunProfile(root);
        TestHost.AppPackage = profile.Capabilities.TryGetValue("appium:appPackage", out var pkg) ? pkg?.ToString() ?? "" : "";

        var driver = DriverFactory.CreateAndroidDriver(profile.AppiumServer, profile.Capabilities);
        TestHost.Driver = driver;

        if (!string.IsNullOrEmpty(TestHost.AppPackage))
        {
            try
            {
                driver.ActivateApp(TestHost.AppPackage);
            }
            catch
            {
                // Session may already be on the target app; keep startup resilient.
            }
        }
    }

    [AfterTestRun]
    public static void AfterTestRun()
    {
        try
        {
            TestHost.Driver?.Quit();
        }
        catch
        {
            // best-effort teardown
        }
    }

    [AfterScenario]
    public void AfterScenario(ScenarioContext scenarioContext)
    {
        if (scenarioContext.TestError is null)
        {
            return;
        }

        var driver = TestHost.Driver;
        if (driver is null)
        {
            return;
        }

        var safeName = scenarioContext.ScenarioInfo.Title
            .Replace(" ", "_")
            .Replace("/", "_")
            .Replace("\\", "_");

        AttachFailureArtifacts(driver, safeName);
    }

    private static void AttachFailureArtifacts(AndroidDriver driver, string testName)
    {
        var screenshotDir = Path.Combine(FindProjectRoot(), "artifacts", "screenshots");
        Directory.CreateDirectory(screenshotDir);

        try
        {
            var screenshot = driver.GetScreenshot();
            var pngPath = Path.Combine(screenshotDir, $"{testName}.png");
            screenshot.SaveAsFile(pngPath);
            try
            {
                AllureApi.AddAttachment($"{testName}_screenshot", "image/png", screenshot.AsByteArray, ".png");
            }
            catch
            {
                // Allure context may be unavailable; file is still saved on disk.
            }
        }
        catch
        {
            // Driver can be half-crashed on failure; never break reporting.
        }

        try
        {
            var source = driver.PageSource ?? "";
            var xmlPath = Path.Combine(screenshotDir, $"{testName}.xml");
            File.WriteAllText(xmlPath, source);
            try
            {
                AllureApi.AddAttachment(
                    $"{testName}_page_source",
                    "application/xml",
                    System.Text.Encoding.UTF8.GetBytes(source),
                    ".xml");
            }
            catch
            {
                // best-effort attach
            }
        }
        catch
        {
            // best-effort page source capture
        }
    }

    private static RunProfile BuildRunProfile(string root)
    {
        var profilePath = Path.Combine(root, ProfileRelativePath.Replace('/', Path.DirectorySeparatorChar));
        var profile = ConfigLoader.LoadProfile(profilePath);

        if (profile.Apk is not null)
        {
            var resolvedApk = ApkResolver.ResolveLatestApk(profile.Apk.BaseFolder, profile.Apk.FixedApkName);
            profile.Capabilities["appium:app"] = resolvedApk;
        }

        var appPackage = profile.Capabilities.TryGetValue("appium:appPackage", out var pkg) ? pkg?.ToString() : null;
        var udid = profile.Capabilities.TryGetValue("appium:udid", out var u) ? u?.ToString() : null;

        if (!string.IsNullOrEmpty(appPackage) && !string.IsNullOrEmpty(udid))
        {
            var resolvedActivity = ActivityResolver.ResolveLaunchActivity(udid!, appPackage!);
            if (!string.IsNullOrEmpty(resolvedActivity))
            {
                profile.Capabilities["appium:appActivity"] = resolvedActivity!;
            }
        }

        if (!profile.Capabilities.ContainsKey("appium:appWaitActivity"))
        {
            profile.Capabilities["appium:appWaitActivity"] = "*";
        }

        if (!profile.Capabilities.ContainsKey("appium:appWaitDuration"))
        {
            profile.Capabilities["appium:appWaitDuration"] = 20000L;
        }

        return profile;
    }

    private static void LoadEnvironment(string root)
    {
        var envPath = Path.Combine(root, ".env");
        if (File.Exists(envPath))
        {
            DotNetEnv.Env.Load(envPath);
        }
    }

    private static string FindProjectRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Shoppix.MobileApp.Automation.csproj")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        return Directory.GetCurrentDirectory();
    }
}
