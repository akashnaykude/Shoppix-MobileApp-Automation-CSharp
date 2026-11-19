using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;

namespace Shoppix.MobileApp.Automation.Support;

/// <summary>Shared helpers used across the step binding classes.</summary>
public static class StepHelpers
{
    public static AndroidDriver Driver => TestHost.Driver;

    public static string AppPackage => TestHost.AppPackage;

    public static bool SafeBack()
    {
        try
        {
            Driver.Navigate().Back();
            return true;
        }
        catch (WebDriverException)
        {
            return false;
        }
    }

    public static void ActivateApp()
    {
        if (string.IsNullOrEmpty(AppPackage))
        {
            return;
        }

        try
        {
            Driver.ActivateApp(AppPackage);
        }
        catch
        {
            // best-effort
        }
    }

    public static void SoftRelaunchApp()
    {
        if (string.IsNullOrEmpty(AppPackage))
        {
            return;
        }

        try
        {
            Driver.TerminateApp(AppPackage);
        }
        catch
        {
            // best-effort
        }

        try
        {
            Driver.ActivateApp(AppPackage);
        }
        catch
        {
            // best-effort
        }
    }

    public static void BackgroundApp(int seconds)
    {
        try
        {
            Driver.BackgroundApp(TimeSpan.FromSeconds(seconds));
        }
        catch
        {
            // Keep non-blocking across device/appium variants.
        }
    }

    public static void ChangeOrientation()
    {
        try
        {
            var rotatable = (IRotatable)Driver;
            var original = rotatable.Orientation;
            var target = original == ScreenOrientation.Portrait ? ScreenOrientation.Landscape : ScreenOrientation.Portrait;
            rotatable.Orientation = target;
            rotatable.Orientation = original;
        }
        catch
        {
            // Device or app can lock orientation; keep scenario non-blocking.
        }
    }
}
