using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using Shoppix.MobileApp.Automation.Core;

namespace Shoppix.MobileApp.Automation.Pages;

/// <summary>
/// Shared page-object behaviour (waits, keyboard, gestures, page-source checks)
/// that the Python pages duplicated across each class.
/// </summary>
public abstract class BasePage
{
    protected readonly AndroidDriver Driver;

    protected BasePage(AndroidDriver driver) => Driver = driver;

    protected AppiumElement Find(By locator, double timeout = 20) => Waits.Find(Driver, locator, timeout);

    protected AppiumElement FindClickable(By locator, double timeout = 20) => Waits.FindClickable(Driver, locator, timeout);

    protected bool IsVisible(By locator, double timeout = 10) => Waits.IsVisible(Driver, locator, timeout);

    protected bool Present(By locator)
    {
        try
        {
            return Driver.FindElements(locator).Count > 0;
        }
        catch
        {
            return false;
        }
    }

    protected IReadOnlyList<AppiumElement> FindAll(By locator)
    {
        try
        {
            return Driver.FindElements(locator).Cast<AppiumElement>().ToList();
        }
        catch
        {
            return new List<AppiumElement>();
        }
    }

    protected string PageSource()
    {
        try
        {
            return Driver.PageSource ?? "";
        }
        catch
        {
            return "";
        }
    }

    protected string PageSourceLower() => PageSource().ToLowerInvariant();

    protected static string? Attr(AppiumElement element, string name)
    {
        try
        {
            return element.GetAttribute(name);
        }
        catch
        {
            return null;
        }
    }

    protected static string AttrLower(AppiumElement element, string name) => (Attr(element, name) ?? "").ToLowerInvariant();

    protected void DismissKeyboardIfNeeded()
    {
        try
        {
            if (Driver.IsKeyboardShown())
            {
                Driver.HideKeyboard();
            }
        }
        catch
        {
            // Keyboard state is best-effort across devices.
        }
    }

    protected void ScrollToText(string text)
    {
        try
        {
            Driver.FindElement(AppiumBy.AndroidUIAutomator(
                $"new UiScrollable(new UiSelector().scrollable(true)).scrollIntoView(new UiSelector().textContains(\"{text}\"))"));
        }
        catch
        {
            // Scrolling is best-effort; target may already be on screen.
        }
    }

    protected void TapXy(int x, int y)
    {
        try
        {
            Driver.ExecuteScript("mobile: clickGesture", new Dictionary<string, object>
            {
                ["x"] = x,
                ["y"] = y,
            });
        }
        catch
        {
            try
            {
                Driver.ExecuteScript("mobile: longClickGesture", new Dictionary<string, object>
                {
                    ["x"] = x,
                    ["y"] = y,
                    ["duration"] = 50,
                });
            }
            catch
            {
                // Tap is best-effort when gesture APIs are unavailable.
            }
        }
    }

    protected static (int X1, int Y1, int X2, int Y2)? ParseBounds(string? bounds)
    {
        var match = Regex.Match(bounds ?? "", @"\[(\d+),(\d+)\]\[(\d+),(\d+)\]");
        if (!match.Success)
        {
            return null;
        }

        return (
            int.Parse(match.Groups[1].Value),
            int.Parse(match.Groups[2].Value),
            int.Parse(match.Groups[3].Value),
            int.Parse(match.Groups[4].Value));
    }

    protected bool TapElementCenter(AppiumElement element)
    {
        var bounds = ParseBounds(Attr(element, "bounds"));
        if (bounds is null)
        {
            return false;
        }

        var (x1, y1, x2, y2) = bounds.Value;
        TapXy((x1 + x2) / 2, (y1 + y2) / 2);
        return true;
    }

    protected bool SafeBack()
    {
        try
        {
            Driver.Navigate().Back();
            return true;
        }
        catch
        {
            return false;
        }
    }

    protected static (int X, int Y, int Width, int Height) WindowRect(AndroidDriver driver)
    {
        var size = driver.Manage().Window.Size;
        try
        {
            var pos = driver.Manage().Window.Position;
            return (pos.X, pos.Y, size.Width, size.Height);
        }
        catch
        {
            return (0, 0, size.Width, size.Height);
        }
    }
}
