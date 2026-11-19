using System.Diagnostics;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;

namespace Shoppix.MobileApp.Automation.Pages;

/// <summary>Port of src/pages/home_page.py.</summary>
public sealed class HomePage : BasePage
{
    private static readonly By HomeHeader = AppiumBy.Id("com.kantarworldpanel.shoppix:id/img_header");
    private static readonly By MenuSettings = AppiumBy.Id("com.kantarworldpanel.shoppix:id/menu_action_setting");
    private static readonly By TutorialSkip = AppiumBy.XPath("//android.widget.TextView[@text='Skip']");
    private static readonly By TutorialTitle = AppiumBy.XPath("//android.widget.TextView[@text='We are Shoppix!']");
    private static readonly By LoadingMessage = AppiumBy.Id("com.kantarworldpanel.shoppix:id/dialog_message");
    private static readonly By SettingsRow = AppiumBy.XPath(
        "//android.widget.TextView[@text='Settings']/ancestor::android.view.View[@clickable='true'][1]");
    private static readonly By DeleteAccountRow = AppiumBy.XPath(
        "//android.widget.TextView[contains(translate(@text,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'delete') and contains(translate(@text,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'account')]/ancestor::android.view.View[@clickable='true'][1]");
    private static readonly By DeleteAccountPasswordInput = AppiumBy.XPath("//android.widget.EditText");
    private static readonly By DeleteMyAccountCta = AppiumBy.XPath(
        "//android.widget.TextView[contains(translate(@text,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'delete my account')]/ancestor::android.view.View[@clickable='true'][1]");
    private static readonly By DeleteConfirmCta = AppiumBy.XPath(
        "//android.widget.TextView[@text='Delete']/ancestor::android.view.View[@clickable='true'][1]");
    private static readonly By DeletePasswordContinueCta = AppiumBy.XPath(
        "//android.widget.TextView[@text='Continue']/ancestor::android.view.View[@clickable='true'][1]");
    private static readonly By OopsTitle = AppiumBy.XPath("//android.widget.TextView[@text='Oops']");
    private static readonly By OopsOk = AppiumBy.XPath("//android.widget.TextView[@text='OK']");
    private static readonly By TooManyRequests = AppiumBy.XPath(
        "//android.widget.TextView[contains(@text,'Too many requests') and contains(@text,'Please try again later')]");
    private static readonly By SignOutRow = AppiumBy.XPath(
        "//android.widget.TextView[@text='Sign Out']/ancestor::android.view.View[@clickable='true'][1]");
    private static readonly By SignOutConfirmText = AppiumBy.XPath(
        "//android.widget.TextView[@text='Are you sure you wish to sign out?']");
    private static readonly By SignOutYes = AppiumBy.XPath("//android.widget.TextView[@text='Yes']");

    public HomePage(AndroidDriver driver) : base(driver)
    {
    }

    private void ScrollToSignOut(int maxSwipes = 8)
    {
        if (IsVisible(SignOutRow, 2))
        {
            return;
        }

        var rect = WindowRect(Driver);
        var left = (int)(rect.X + rect.Width * 0.08);
        var top = (int)(rect.Y + rect.Height * 0.25);
        var width = (int)(rect.Width * 0.84);
        var height = (int)(rect.Height * 0.55);

        for (var i = 0; i < maxSwipes; i++)
        {
            Driver.ExecuteScript("mobile: scrollGesture", new Dictionary<string, object>
            {
                ["left"] = left,
                ["top"] = top,
                ["width"] = width,
                ["height"] = height,
                ["direction"] = "down",
                ["percent"] = 0.85,
            });
            if (IsVisible(SignOutRow, 2))
            {
                return;
            }
        }

        Driver.FindElement(AppiumBy.AndroidUIAutomator(
            "new UiScrollable(new UiSelector().scrollable(true)).scrollIntoView(new UiSelector().text(\"Sign Out\"));"));
    }

    public bool IsHomeHeaderVisible(double timeout = 20) => IsVisible(HomeHeader, timeout);

    public bool IsTutorialVisible(double timeout = 2) => Present(TutorialSkip) || Present(TutorialTitle);

    public bool IsLoadingVisible(double timeout = 2)
    {
        if (IsVisible(LoadingMessage, timeout))
        {
            return true;
        }

        return PageSourceLower().Contains("loading...");
    }

    public bool IsTooManyRequestsVisible(double timeout = 2)
    {
        if (!Present(OopsTitle))
        {
            return false;
        }

        if (Present(TooManyRequests))
        {
            return true;
        }

        var src = PageSourceLower();
        return src.Contains("too many requests") && src.Contains("please try again later");
    }

    public bool IsHomeLikeState()
    {
        var src = PageSourceLower();
        if (src.Contains("menu_action_setting") || src.Contains("img_header"))
        {
            return true;
        }

        if (src.Contains("my profile") && src.Contains("settings") && src.Contains("rewards"))
        {
            return true;
        }

        if (src.Contains("navigate to settings") && src.Contains("privacy policy"))
        {
            return true;
        }

        return false;
    }

    public bool IsHomeDashboardVisible(double timeout = 3)
    {
        if (IsVisible(HomeHeader, timeout) || Present(HomeHeader))
        {
            return true;
        }

        var src = PageSourceLower();
        string[] navTabs = { "home", "history", "receipt", "online", "surveys" };
        return navTabs.Count(tab => src.Contains(tab)) >= 3;
    }

    public bool DismissOopsIfPresent(double timeout = 2)
    {
        if (!IsVisible(OopsTitle, timeout))
        {
            return false;
        }

        try
        {
            FindClickable(OopsOk, 3).Click();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string WaitForPostRegistrationState(double timeout = 35)
    {
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed.TotalSeconds < timeout)
        {
            if (IsTooManyRequestsVisible())
            {
                return "rate_limited";
            }

            if (Present(HomeHeader) || IsHomeLikeState())
            {
                return "home";
            }

            if (IsTutorialVisible())
            {
                return "tutorial";
            }

            Thread.Sleep(500);
        }

        return "unknown";
    }

    public bool SkipTutorialIfPresent(double timeout = 25)
    {
        if (!IsTutorialVisible())
        {
            return IsHomeHeaderVisible(4);
        }

        FindClickable(TutorialSkip, 6).Click();
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed.TotalSeconds < timeout)
        {
            if (Present(HomeHeader))
            {
                return true;
            }

            Thread.Sleep(500);
        }

        return false;
    }

    public void OpenSettingsFromHome()
    {
        if (!Present(SettingsRow))
        {
            FindClickable(MenuSettings, 20).Click();
        }

        FindClickable(SettingsRow, 12).Click();
    }

    public bool DeleteAccountFromSettings(string password)
    {
        ScrollToText("Delete");
        try
        {
            FindClickable(DeleteAccountRow, 8).Click();
        }
        catch
        {
            return false;
        }

        var passwordEntered = false;
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed.TotalSeconds < 45)
        {
            var src = PageSourceLower();

            if (new[] { "get started", "login as existing user", "welcome back" }.Any(src.Contains))
            {
                return true;
            }

            if (src.Contains("are you sure") && Present(DeleteConfirmCta))
            {
                try
                {
                    FindClickable(DeleteConfirmCta, 3).Click();
                    continue;
                }
                catch
                {
                    // retry loop
                }
            }

            if (Present(DeleteAccountPasswordInput))
            {
                if (!passwordEntered)
                {
                    DismissKeyboardIfNeeded();
                    try
                    {
                        var field = Find(DeleteAccountPasswordInput, 3);
                        field.Clear();
                        field.SendKeys(password);
                        passwordEntered = true;
                    }
                    catch
                    {
                        // retry loop
                    }

                    DismissKeyboardIfNeeded();
                }

                try
                {
                    FindClickable(DeletePasswordContinueCta, 4).Click();
                    continue;
                }
                catch
                {
                    // retry loop
                }
            }

            if (Present(DeleteMyAccountCta))
            {
                try
                {
                    FindClickable(DeleteMyAccountCta, 3).Click();
                    continue;
                }
                catch
                {
                    // retry loop
                }
            }

            Thread.Sleep(400);
        }

        return false;
    }

    public bool CleanupNewlyRegisteredAccount(string password)
    {
        if (IsTutorialVisible(2))
        {
            if (!SkipTutorialIfPresent(25))
            {
                return false;
            }
        }

        if (!IsHomeHeaderVisible(10))
        {
            return false;
        }

        OpenSettingsFromHome();
        return DeleteAccountFromSettings(password);
    }

    public void LogoutFromMenu()
    {
        FindClickable(MenuSettings, 20).Click();

        ScrollToSignOut();
        FindClickable(SignOutRow, 12).Click();

        Find(SignOutConfirmText, 12);
        FindClickable(SignOutYes, 8).Click();
    }
}
