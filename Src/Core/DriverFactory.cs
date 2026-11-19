using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;

namespace Shoppix.MobileApp.Automation.Core;

/// <summary>Creates an Android Appium driver. Mirrors src/core/driver_factory.py.</summary>
public static class DriverFactory
{
    public static AndroidDriver CreateAndroidDriver(string serverUrl, Dictionary<string, object?> capabilities)
    {
        var options = new AppiumOptions();

        foreach (var (key, value) in capabilities)
        {
            if (value is null)
            {
                continue;
            }

            // Strip the "appium:" vendor prefix so reserved caps can be matched.
            var name = key.StartsWith("appium:", StringComparison.Ordinal) ? key.Substring("appium:".Length) : key;

            // Appium's AppiumOptions reserves these capabilities and rejects them
            // via AddAdditionalAppiumOption; they must be set through properties.
            switch (name)
            {
                case "platformName":
                    options.PlatformName = value.ToString();
                    break;
                case "automationName":
                    options.AutomationName = value.ToString();
                    break;
                case "deviceName":
                    options.DeviceName = value.ToString();
                    break;
                case "platformVersion":
                    options.PlatformVersion = value.ToString();
                    break;
                case "app":
                    options.App = value.ToString();
                    break;
                default:
                    options.AddAdditionalAppiumOption(name, value);
                    break;
            }
        }

        return new AndroidDriver(new Uri(serverUrl), options, TimeSpan.FromSeconds(180));
    }
}
