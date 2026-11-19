using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Support.UI;

namespace Shoppix.MobileApp.Automation.Core;

/// <summary>
/// Explicit-wait helpers equivalent to Selenium's expected_conditions used by
/// the Python page objects (presence_of, element_to_be_clickable, visibility_of).
/// </summary>
public static class Waits
{
    private static WebDriverWait NewWait(AndroidDriver driver, double timeoutSeconds)
    {
        return new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
    }

    /// <summary>presence_of_element_located</summary>
    public static AppiumElement Find(AndroidDriver driver, By locator, double timeout = 20)
    {
        return NewWait(driver, timeout).Until(d =>
        {
            var elements = d.FindElements(locator);
            return elements.Count > 0 ? (AppiumElement)elements[0] : null;
        })!;
    }

    /// <summary>element_to_be_clickable</summary>
    public static AppiumElement FindClickable(AndroidDriver driver, By locator, double timeout = 20)
    {
        return NewWait(driver, timeout).Until(d =>
        {
            var elements = d.FindElements(locator);
            if (elements.Count == 0)
            {
                return null;
            }

            var element = elements[0];
            try
            {
                return element.Displayed && element.Enabled ? (AppiumElement)element : null;
            }
            catch
            {
                return null;
            }
        })!;
    }

    /// <summary>visibility_of_element_located wrapped to return a boolean.</summary>
    public static bool IsVisible(AndroidDriver driver, By locator, double timeout = 10)
    {
        try
        {
            NewWait(driver, timeout).Until(d =>
            {
                var elements = d.FindElements(locator);
                return elements.Count > 0 && elements[0].Displayed;
            });
            return true;
        }
        catch
        {
            return false;
        }
    }
}
