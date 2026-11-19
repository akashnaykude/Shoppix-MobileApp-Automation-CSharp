using System.Diagnostics;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;

namespace Shoppix.MobileApp.Automation.Pages;

/// <summary>Port of src/pages/login_page.py.</summary>
public sealed class LoginPage : BasePage
{
    private static readonly By AppLogo = AppiumBy.XPath("//android.widget.ImageView[@content-desc='Kantar Logo']");
    private static readonly By WelcomeTitle = AppiumBy.XPath("//android.widget.TextView[@text='Welcome Back!']");
    private static readonly By Subtitle = AppiumBy.XPath(
        "//android.widget.TextView[contains(@text, 'Submit receipts') and contains(@text, 'earn rewards')]");
    private static readonly By EmailInput = AppiumBy.XPath("//android.widget.EditText[1]");
    private static readonly By PasswordInput = AppiumBy.XPath("//android.widget.EditText[2]");
    private static readonly By PasswordToggle = AppiumBy.XPath("//android.widget.EditText[2]//android.widget.Button");
    private static readonly By LoginText = AppiumBy.XPath("//android.widget.TextView[@text='Login']");
    private static readonly By LoginContainer = AppiumBy.XPath(
        "//android.widget.TextView[@text='Login']/ancestor::android.view.View[@clickable='true'][1]");

    private static readonly By WelcomeLoginCta = AppiumBy.XPath(
        "//android.widget.TextView[@text='Login as existing User']/ancestor::android.view.View[@clickable='true'][1]");

    private static readonly By AlertTitle = AppiumBy.XPath("//android.widget.TextView[@text='Sorry']");
    private static readonly By AlertOk = AppiumBy.XPath("//android.widget.TextView[@text='OK']");

    private static readonly By EmailPlaceholder = AppiumBy.XPath("//android.widget.TextView[@text='Enter your email']");
    private static readonly By PasswordPlaceholder = AppiumBy.XPath("//android.widget.TextView[@text='Enter your password']");
    private static readonly By EmailLabel = AppiumBy.XPath("//android.widget.TextView[@text='Email']");
    private static readonly By PasswordLabel = AppiumBy.XPath("//android.widget.TextView[@text='Password']");
    private static readonly By ForgotPassword = AppiumBy.XPath("//android.widget.TextView[contains(@text, 'Forgot Password')]");
    private static readonly By ForgotPasswordHeading = AppiumBy.XPath("//android.widget.TextView[contains(@text, 'Forgot your password')]");
    private static readonly By ForgotPasswordDescription = AppiumBy.XPath(
        "//android.widget.TextView[contains(@text, 'send you a link to reset your password')]");
    private static readonly By BackButton = AppiumBy.XPath("//android.widget.ImageView[@content-desc='Back']");
    private static readonly By SocialLoginText = AppiumBy.XPath("//android.widget.TextView[contains(@text, 'or login with')]");
    private static readonly By FacebookLogin = AppiumBy.XPath(
        "//android.widget.ImageView[@content-desc='Connect with Facebook' or @content-desc='Register with Facebook']");
    private static readonly By SignUpLink = AppiumBy.XPath("//android.widget.TextView[contains(@text, 'Sign up')]");
    private static readonly By SignUpCta = AppiumBy.XPath(
        "//android.widget.TextView[contains(@text, 'Sign up')]/ancestor::android.view.View[@clickable='true'][1]");
    private static readonly By EmailRequiredError = AppiumBy.XPath("//android.widget.TextView[contains(@text, 'Please enter an email')]");
    private static readonly By PasswordRequiredError = AppiumBy.XPath("//android.widget.TextView[contains(@text, 'Please enter a password')]");
    private static readonly By InvalidEmailError = AppiumBy.XPath(
        "//android.widget.TextView[contains(@text, 'valid email') or contains(@text, 'Invalid')]");
    private static readonly By OopsOk = AppiumBy.XPath("//android.widget.TextView[@text='OK']");

    public LoginPage(AndroidDriver driver) : base(driver)
    {
    }

    private AppiumElement FindPresence(By locator, double timeout = 20) => Find(locator, timeout);

    public void NavigateToLoginIfNeeded()
    {
        DismissOopsIfPresent();

        var src = PageSourceLower();

        if (src.Contains("welcome back!") || src.Contains("enter your email"))
        {
            return;
        }

        if (src.Contains("login as existing user") || src.Contains("let's get started") || src.Contains("lets get started"))
        {
            try
            {
                FindClickable(WelcomeLoginCta, 5).Click();
                FindPresence(WelcomeTitle, 10);
                return;
            }
            catch
            {
                // fall through
            }
        }

        if (IsVisible(WelcomeTitle, 2))
        {
            return;
        }

        if (IsVisible(WelcomeLoginCta, 3))
        {
            FindClickable(WelcomeLoginCta, 4).Click();
            FindPresence(WelcomeTitle, 10);
        }
    }

    public bool IsLoginScreenVisible(double timeout = 10) => IsVisible(WelcomeTitle, timeout);

    public bool AreFieldLabelsVisible() => IsVisible(EmailLabel, 6) && IsVisible(PasswordLabel, 6);

    public bool IsEmailFieldReady()
    {
        if (IsVisible(EmailPlaceholder, 2))
        {
            return true;
        }

        try
        {
            var text = (Attr(FindPresence(EmailInput, 5), "text") ?? "").Trim();
            return text.Length > 0;
        }
        catch
        {
            return false;
        }
    }

    public bool IsPasswordPlaceholderVisible() => IsVisible(PasswordPlaceholder, 6);

    public void EnterEmail(string email)
    {
        var element = FindPresence(EmailInput, 15);
        element.Clear();
        element.SendKeys(email);
    }

    public void EnterPassword(string password)
    {
        var element = FindPresence(PasswordInput, 15);
        element.Clear();
        element.SendKeys(password);
    }

    public void ResetForm()
    {
        try
        {
            FindPresence(EmailInput, 5).Clear();
        }
        catch
        {
            // best-effort
        }

        try
        {
            FindPresence(PasswordInput, 5).Clear();
        }
        catch
        {
            // best-effort
        }
    }

    public bool TapLogin()
    {
        DismissKeyboardIfNeeded();

        try
        {
            var element = FindPresence(LoginContainer, 2);
            if ((Attr(element, "enabled") ?? "").ToLowerInvariant() != "true")
            {
                return false;
            }

            try
            {
                element.Click();
                return true;
            }
            catch
            {
                FindClickable(LoginContainer, 2).Click();
                return true;
            }
        }
        catch
        {
            // fall through
        }

        try
        {
            FindClickable(LoginText, 2).Click();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool IsLoginButtonEnabled()
    {
        try
        {
            var element = FindPresence(LoginContainer, 6);
            return (Attr(element, "enabled") ?? "").ToLowerInvariant() == "true";
        }
        catch
        {
            return false;
        }
    }

    public void LoginWithCredentials(string email, string password)
    {
        EnterEmail(email);
        EnterPassword(password);
        TapLogin();
    }

    public (string Title, string Message) GetInvalidAlert(IReadOnlyList<string> allowedMessages, double timeout = 15)
    {
        FindPresence(AlertTitle, timeout);
        foreach (var message in allowedMessages)
        {
            var locator = AppiumBy.XPath($"//android.widget.TextView[@text=\"{message}\"]");
            if (IsVisible(locator, 3))
            {
                return ("Sorry", message);
            }
        }

        return ("Sorry", "");
    }

    public void DismissInvalidAlert() => FindPresence(AlertOk, 10).Click();

    public bool IsInvalidAlertVisible(double timeout = 5) => IsVisible(AlertTitle, timeout);

    public bool IsLogoVisible(double timeout = 6) => IsVisible(AppLogo, timeout);

    public bool IsHeadingVisible(double timeout = 6) => IsVisible(WelcomeTitle, timeout);

    public bool IsSubtitleVisible(double timeout = 6) => IsVisible(Subtitle, timeout);

    public bool IsEmailLabelVisible(double timeout = 6) => IsVisible(EmailLabel, timeout);

    public bool IsPasswordLabelVisible(double timeout = 6) => IsVisible(PasswordLabel, timeout);

    public bool IsEmailPlaceholderVisible(double timeout = 6) => IsVisible(EmailPlaceholder, timeout);

    public bool IsPasswordToggleVisible(double timeout = 6) => IsVisible(PasswordToggle, timeout);

    public bool IsForgotPasswordVisible(double timeout = 6) => IsVisible(ForgotPassword, timeout);

    public bool IsLoginButtonVisible(double timeout = 6) => IsVisible(LoginText, timeout);

    public bool IsSocialLoginTextVisible(double timeout = 6) => IsVisible(SocialLoginText, timeout);

    public bool IsFacebookLoginButtonVisible(double timeout = 6) => IsVisible(FacebookLogin, timeout);

    public bool IsSignUpLinkVisible(double timeout = 6) => IsVisible(SignUpLink, timeout);

    public string GetEmailValue()
    {
        try
        {
            return (Attr(FindPresence(EmailInput, 5), "text") ?? "").Trim();
        }
        catch
        {
            return "";
        }
    }

    public bool IsPasswordMasked()
    {
        try
        {
            var element = FindPresence(PasswordInput, 5);
            var attrPassword = AttrLower(element, "password") == "true";
            var textValue = Attr(element, "text") ?? "";
            return attrPassword || textValue.Contains('\u2022');
        }
        catch
        {
            return false;
        }
    }

    public void TapPasswordToggle()
    {
        DismissKeyboardIfNeeded();

        try
        {
            FindClickable(PasswordToggle, 4).Click();
            return;
        }
        catch
        {
            // fall through
        }

        var elem = FindPresence(PasswordToggle, 4);
        try
        {
            elem.Click();
        }
        catch
        {
            // best-effort
        }
    }

    public bool IsPasswordVisibleNow()
    {
        try
        {
            return AttrLower(FindPresence(PasswordInput, 5), "password") == "false";
        }
        catch
        {
            return false;
        }
    }

    public string GetPasswordValue()
    {
        try
        {
            return (Attr(FindPresence(PasswordInput, 4), "text") ?? "").Trim();
        }
        catch
        {
            return "";
        }
    }

    public bool IsPasswordVisibleAs(string expectedPassword)
    {
        try
        {
            var element = FindPresence(PasswordInput, 4);
            var isVisibleAttr = AttrLower(element, "password") == "false";
            var value = (Attr(element, "text") ?? "").Trim();
            return isVisibleAttr && value == expectedPassword;
        }
        catch
        {
            return false;
        }
    }

    public bool IsPasswordHiddenFrom(string plainPassword)
    {
        try
        {
            var element = FindPresence(PasswordInput, 4);
            var isMaskedAttr = AttrLower(element, "password") == "true";
            var value = (Attr(element, "text") ?? "").Trim();
            return isMaskedAttr || value != plainPassword;
        }
        catch
        {
            return false;
        }
    }

    public bool WaitUntilPasswordVisibleAs(string expectedPassword, double timeout = 2.5)
    {
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed.TotalSeconds < timeout)
        {
            if (IsPasswordVisibleAs(expectedPassword))
            {
                return true;
            }

            Thread.Sleep(200);
        }

        return false;
    }

    public bool WaitUntilPasswordHiddenFrom(string plainPassword, double timeout = 2.5)
    {
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed.TotalSeconds < timeout)
        {
            if (IsPasswordHiddenFrom(plainPassword))
            {
                return true;
            }

            Thread.Sleep(200);
        }

        return false;
    }

    public void TapForgotPassword() => FindClickable(ForgotPassword, 8).Click();

    public void TapFacebookLogin() => FindClickable(FacebookLogin, 8).Click();

    public void TapSignUp()
    {
        DismissKeyboardIfNeeded();

        try
        {
            FindPresence(SignUpLink, 1).Click();
            return;
        }
        catch
        {
            // fall through
        }

        try
        {
            FindClickable(SignUpCta, 1).Click();
            return;
        }
        catch
        {
            // fall through
        }

        try
        {
            FindClickable(SignUpLink, 1).Click();
            return;
        }
        catch
        {
            // fall through
        }

        try
        {
            Driver.FindElement(AppiumBy.AndroidUIAutomator(
                "new UiScrollable(new UiSelector().scrollable(true)).scrollIntoView(new UiSelector().textContains(\"Sign up\"))"));
        }
        catch
        {
            // best-effort scroll
        }

        try
        {
            FindClickable(SignUpCta, 2).Click();
        }
        catch
        {
            FindClickable(SignUpLink, 2).Click();
        }
    }

    public bool IsForgotPasswordScreenVisible()
    {
        var headingVisible = IsVisible(ForgotPasswordHeading, 8);
        if (!headingVisible)
        {
            return false;
        }

        var descriptionVisible = IsVisible(ForgotPasswordDescription, 3);
        var backVisible = IsVisible(BackButton, 3);
        return descriptionVisible || backVisible;
    }

    public bool IsSignUpScreenVisible()
    {
        if (IsLoginScreenVisible(1))
        {
            return false;
        }

        var src = PageSourceLower();
        return src.Contains("sign up") || src.Contains("register") || src.Contains("let's get started") || src.Contains("lets get started");
    }

    public bool IsEmailFormatErrorVisible()
    {
        if (IsVisible(InvalidEmailError, 3))
        {
            return true;
        }

        var src = PageSourceLower();
        return src.Contains("invalid") && src.Contains("email");
    }

    public bool IsEmailRequiredErrorVisible()
    {
        if (IsVisible(EmailRequiredError, 3))
        {
            return true;
        }

        var src = PageSourceLower();
        if (src.Contains("please enter an email") || src.Contains("please enter email"))
        {
            return true;
        }

        if (src.Contains("content-invalid=\"true\"") && src.Contains("enter your email"))
        {
            return string.IsNullOrEmpty(GetEmailValue());
        }

        return false;
    }

    public bool IsPasswordRequiredErrorVisible()
    {
        if (IsVisible(PasswordRequiredError, 3))
        {
            return true;
        }

        var src = PageSourceLower();
        if (src.Contains("please enter a password") || src.Contains("please enter password"))
        {
            return true;
        }

        if (src.Contains("content-invalid=\"true\"") && src.Contains("enter your password"))
        {
            try
            {
                var val = (Attr(FindPresence(PasswordInput, 2), "text") ?? "").Trim();
                return val == "";
            }
            catch
            {
                return true;
            }
        }

        return false;
    }

    public bool DismissOopsIfPresent()
    {
        var src = PageSourceLower();
        if (!src.Contains("oops") && !src.Contains("connection error"))
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

    public bool HasBasicAccessibilityLabels()
    {
        var src = PageSource();
        string[] checks = { "Kantar Logo", "Connect with Facebook", "Email", "Password", "Login" };
        return checks.Where(token => token.Length > 0).All(src.Contains);
    }
}
