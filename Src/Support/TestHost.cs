using OpenQA.Selenium.Appium.Android;

namespace Shoppix.MobileApp.Automation.Support;

/// <summary>
/// Session-scoped shared state, equivalent to the pytest session fixtures
/// (driver, env_setup, login_data). Created once per test run in <see cref="Hooks"/>.
/// </summary>
public static class TestHost
{
    public static AndroidDriver Driver { get; set; } = null!;
    public static string AppPackage { get; set; } = "";
    public static string Email { get; set; } = "";
    public static string Password { get; set; } = "";
    public static Core.LoginData LoginData { get; set; } = null!;
}
