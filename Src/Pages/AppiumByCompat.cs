using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace Shoppix.MobileApp.Automation.Pages;

/// <summary>
/// Compatibility shim that exposes the modern Appium <c>AppiumBy</c> factory API
/// on top of Appium.WebDriver 5.2.0 (which still ships the legacy <c>By*</c>
/// locator classes). Keeps page objects reading like the Python Appium locators.
/// </summary>
internal static class AppiumBy
{
    public static By XPath(string value) => OpenQA.Selenium.By.XPath(value);

    public static By Id(string value) => new ById(value);

    public static By AccessibilityId(string value) => new ByAccessibilityId(value);

    public static By AndroidUIAutomator(string value) => new ByAndroidUIAutomator(value);
}
