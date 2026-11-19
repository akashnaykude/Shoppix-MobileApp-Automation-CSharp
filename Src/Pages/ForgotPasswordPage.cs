using System.Diagnostics;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;

namespace Shoppix.MobileApp.Automation.Pages;

/// <summary>Page object for the "Forgot your password?" screen reached from Login.</summary>
public sealed class ForgotPasswordPage : BasePage
{
    private static readonly By Heading = AppiumBy.XPath("//android.widget.TextView[contains(@text, 'Forgot your password')]");
    private static readonly By Description = AppiumBy.XPath(
        "//android.widget.TextView[contains(@text, 'send you a link') or contains(@text, 'reset your password')]");
    private static readonly By EmailLabel = AppiumBy.XPath("//android.widget.TextView[@text='Email']");
    private static readonly By EmailInput = AppiumBy.XPath("//android.widget.EditText[1]");
    private static readonly By EmailPlaceholder = AppiumBy.XPath("//android.widget.TextView[@text='Enter your email']");
    private static readonly By SendText = AppiumBy.XPath("//android.widget.TextView[@text='Send']");
    private static readonly By SendContainer = AppiumBy.XPath(
        "//android.widget.TextView[@text='Send']/ancestor::android.view.View[@clickable='true'][1]");

    private static readonly By BackButton = AppiumBy.XPath("//android.widget.ImageView[@content-desc='Back']");
    private static readonly By ConfirmationMessage = AppiumBy.XPath(
        "//android.widget.TextView[contains(@text, 'you will receive an email shortly') "
        + "or contains(@text, 'associated with an account')]");
    private static readonly By GoToLoginText = AppiumBy.XPath("//android.widget.TextView[@text='Go to login']");
    private static readonly By GoToLoginContainer = AppiumBy.XPath(
        "//android.widget.TextView[@text='Go to login']/ancestor::android.view.View[@clickable='true'][1]");

    public ForgotPasswordPage(AndroidDriver driver) : base(driver)
    {
    }

    public bool IsForgotPasswordScreenVisible(double timeout = 8)
    {
        if (!IsVisible(Heading, timeout))
        {
            return false;
        }

        // Send/Back are stable screen markers; the placeholder only shows when the
        // email field is empty, so check it last to avoid an unnecessary wait.
        return IsVisible(SendText, 3) || IsVisible(BackButton, 3) || IsVisible(EmailPlaceholder, 3);
    }

    public bool IsHeadingVisible(double timeout = 6) => IsVisible(Heading, timeout);

    public bool IsDescriptionVisible(double timeout = 6) => IsVisible(Description, timeout);

    public bool IsEmailLabelVisible(double timeout = 6) => IsVisible(EmailLabel, timeout);

    public bool IsEmailPlaceholderVisible(double timeout = 6) => IsVisible(EmailPlaceholder, timeout);

    public bool IsSendButtonVisible(double timeout = 6) => IsVisible(SendText, timeout);

    public bool IsBackButtonVisible(double timeout = 6) => IsVisible(BackButton, timeout);

    public bool DisplaysExpectedUiContent() =>
        IsHeadingVisible() && IsDescriptionVisible() && IsEmailLabelVisible() && IsSendButtonVisible();

    public void EnterEmail(string email)
    {
        var element = Find(EmailInput, 15);
        element.Clear();
        if (!string.IsNullOrEmpty(email))
        {
            element.SendKeys(email);
        }
    }

    public void ResetForm()
    {
        try
        {
            Find(EmailInput, 5).Clear();
        }
        catch
        {
            // best-effort
        }

        DismissKeyboardIfNeeded();
    }

    public string GetEmailValue()
    {
        try
        {
            return (Attr(Find(EmailInput, 5), "text") ?? "").Trim();
        }
        catch
        {
            return "";
        }
    }

    public bool IsSendButtonEnabled()
    {
        // When the button is disabled there is no clickable ancestor, so a one-shot
        // lookup avoids blocking for the full wait timeout on the negative path.
        var elements = FindAll(SendContainer);
        if (elements.Count == 0)
        {
            return false;
        }

        return AttrLower(elements[0], "enabled") == "true";
    }

    public bool WaitUntilSendEnabled(double timeout = 6)
    {
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed.TotalSeconds < timeout)
        {
            if (IsSendButtonEnabled())
            {
                return true;
            }

            Thread.Sleep(300);
        }

        return false;
    }

    public bool TapSend()
    {
        DismissKeyboardIfNeeded();

        try
        {
            var element = Find(SendContainer, 3);
            if (AttrLower(element, "enabled") != "true")
            {
                return false;
            }

            element.Click();
            return true;
        }
        catch
        {
            // fall through
        }

        try
        {
            FindClickable(SendText, 2).Click();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool IsConfirmationAlertVisible(double timeout = 15) => IsVisible(ConfirmationMessage, timeout);

    public void TapGoToLogin()
    {
        try
        {
            FindClickable(GoToLoginContainer, 5).Click();
            return;
        }
        catch
        {
            // fall through
        }

        FindClickable(GoToLoginText, 5).Click();
    }

    public void TapBack() => FindClickable(BackButton, 8).Click();
}
