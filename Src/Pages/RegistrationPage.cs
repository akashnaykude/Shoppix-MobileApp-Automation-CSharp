using System.Diagnostics;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;

namespace Shoppix.MobileApp.Automation.Pages;

/// <summary>Port of src/pages/registration_page.py.</summary>
public sealed class RegistrationPage : BasePage
{
    private static readonly By Logo = AppiumBy.XPath("//android.widget.ImageView[@content-desc='Kantar Logo']");
    private static readonly By Heading = AppiumBy.XPath("//android.widget.TextView[@text='Welcome to Shoppix!']");
    private static readonly By Subtitle = AppiumBy.XPath(
        "//android.widget.TextView[contains(@text, 'Start snapping receipts') and contains(@text, 'earning rewards')]");
    private static readonly By BackButton = AppiumBy.XPath("//android.widget.ImageView[@content-desc='Back']");

    private static readonly By EmailLabel = AppiumBy.XPath("//android.widget.TextView[@text='Email']");
    private static readonly By CreatePasswordLabel = AppiumBy.XPath("//android.widget.TextView[@text='Create password']");
    private static readonly By ConfirmPasswordLabel = AppiumBy.XPath("//android.widget.TextView[@text='Confirm Password']");
    private static readonly By ReferralLabel = AppiumBy.XPath("//android.widget.TextView[@text='Have a referral code?']");

    private static readonly By EmailInput = AppiumBy.XPath("(//android.widget.EditText)[1]");
    private static readonly By CreatePasswordInput = AppiumBy.XPath("(//android.widget.EditText)[2]");
    private static readonly By ConfirmPasswordInput = AppiumBy.XPath("(//android.widget.EditText)[3]");
    private static readonly By ReferralInput = AppiumBy.XPath("(//android.widget.EditText)[4]");

    private static readonly By EmailPlaceholder = AppiumBy.XPath("//android.widget.TextView[@text='Enter your email']");
    private static readonly By CreatePasswordPlaceholder = AppiumBy.XPath("//android.widget.TextView[@text='Create your password']");
    private static readonly By ConfirmPasswordPlaceholder = AppiumBy.XPath("//android.widget.TextView[@text='Confirm your password']");
    private static readonly By ReferralPlaceholder = AppiumBy.XPath("//android.widget.TextView[@text='Enter referral code']");

    private static readonly By CreatePasswordToggle = AppiumBy.XPath("(//android.widget.EditText)[2]//android.widget.Button");
    private static readonly By ConfirmPasswordToggle = AppiumBy.XPath("(//android.widget.EditText)[3]//android.widget.Button");

    private static readonly By AgeCheckbox = AppiumBy.XPath("//android.widget.CheckBox");
    private static readonly By ConsentText = AppiumBy.XPath(
        "//android.widget.TextView[contains(@text, 'Terms') and contains(@text, 'Privacy Policy')]");

    private static readonly By ContinueText = AppiumBy.XPath("//android.widget.TextView[@text='Continue']");
    private static readonly By ContinueCta = AppiumBy.XPath(
        "//android.widget.TextView[@text='Continue']/ancestor::android.view.View[@clickable='true'][1]");
    private static readonly By ContinueButton = AppiumBy.XPath(
        "//android.widget.Button[ancestor::*[.//android.widget.TextView[@text='Continue']]]");

    private static readonly By SocialRegText = AppiumBy.XPath("//android.widget.TextView[contains(@text, 'or register with')]");
    private static readonly By FacebookButton = AppiumBy.XPath(
        "//android.widget.ImageView[@content-desc='Connect with Facebook' or @content-desc='Register with Facebook']");

    private static readonly By TermsScreenTitle = AppiumBy.XPath("//android.widget.TextView[@text='Terms and Conditions']");
    private static readonly By PrivacyScreenTitle = AppiumBy.XPath("//android.widget.TextView[@text='Privacy Policy']");
    private static readonly By LegalWebview = AppiumBy.XPath("//android.webkit.WebView");

    private static readonly By PasswordWeakError = AppiumBy.XPath("//android.widget.TextView[contains(@text, 'Please check your password')]");
    private static readonly By PasswordPolicyError = AppiumBy.XPath(
        "//android.widget.TextView[contains(@text, 'at least 8 characters') or contains(@text, 'upper case')]");
    private static readonly By PasswordMismatchError = AppiumBy.XPath(
        "//android.widget.TextView[contains(@text, 'confirm password are different')]");
    private static readonly By EmailRequiredError = AppiumBy.XPath("//android.widget.TextView[contains(@text, 'Please enter an email')]");
    private static readonly By EmailInvalidError = AppiumBy.XPath(
        "//android.widget.TextView[contains(@text, 'valid email') or contains(@text, 'Invalid')]");

    private static readonly By ConsentRequiredError = AppiumBy.XPath(
        "//android.widget.TextView[contains(@text, 'Please confirm you are aged 18 or older')]");

    private static readonly By OopsTitle = AppiumBy.XPath("//android.widget.TextView[@text='Oops']");
    private static readonly By AlreadyRegisteredMessage = AppiumBy.XPath("//android.widget.TextView[contains(@text, 'already registered')]");
    private static readonly By OopsOkText = AppiumBy.XPath("//android.widget.TextView[@text='OK']");

    private List<string> _lastAccessibilityMissing = new();

    public RegistrationPage(AndroidDriver driver) : base(driver)
    {
    }

    private AppiumElement RFind(By locator, double timeout = 15) => Find(locator, timeout);

    private AppiumElement RClickable(By locator, double timeout = 15) => FindClickable(locator, timeout);

    private bool RVisible(By locator, double timeout = 6) => IsVisible(locator, timeout);

    public void NavigateToRegistrationIfNeeded()
    {
        if (IsRegistrationScreenVisible(3))
        {
            return;
        }

        var welcome = new WelcomePage(Driver);
        if (welcome.IsWelcomeVisible(3))
        {
            welcome.TapEmailRegistrationIcon();
            return;
        }

        var login = new LoginPage(Driver);
        if (login.IsLoginScreenVisible(3))
        {
            login.TapSignUp();
            return;
        }

        SafeBack();

        if (welcome.IsWelcomeVisible(3))
        {
            welcome.TapEmailRegistrationIcon();
        }
    }

    public bool IsRegistrationScreenVisible(double timeout = 10) =>
        RVisible(Heading, timeout) && RVisible(EmailLabel, timeout);

    public void ResetForm()
    {
        foreach (var locator in new[] { EmailInput, CreatePasswordInput, ConfirmPasswordInput, ReferralInput })
        {
            try
            {
                RFind(locator, 3).Clear();
            }
            catch
            {
                // best-effort
            }
        }
    }

    public bool IsLogoVisible(double timeout = 5) => RVisible(Logo, timeout);

    public bool IsHeadingVisible(double timeout = 5) => RVisible(Heading, timeout);

    public bool IsSubtitleVisible(double timeout = 5) => RVisible(Subtitle, timeout);

    public bool IsBackButtonVisible(double timeout = 5) => RVisible(BackButton, timeout);

    public bool IsEmailLabelVisible(double timeout = 5) => RVisible(EmailLabel, timeout);

    public bool IsEmailPlaceholderVisible(double timeout = 5) => RVisible(EmailPlaceholder, timeout);

    public bool IsCreatePasswordLabelVisible(double timeout = 5) => RVisible(CreatePasswordLabel, timeout);

    public bool IsCreatePasswordPlaceholderVisible(double timeout = 5) => RVisible(CreatePasswordPlaceholder, timeout);

    public bool IsConfirmPasswordLabelVisible(double timeout = 5) => RVisible(ConfirmPasswordLabel, timeout);

    public bool IsConfirmPasswordPlaceholderVisible(double timeout = 5) => RVisible(ConfirmPasswordPlaceholder, timeout);

    public bool IsReferralLabelVisible(double timeout = 5)
    {
        ScrollToText("Have a referral code");
        return RVisible(ReferralLabel, timeout);
    }

    public bool IsReferralPlaceholderVisible(double timeout = 5)
    {
        ScrollToText("Have a referral code");
        return RVisible(ReferralPlaceholder, timeout);
    }

    public bool IsAgeCheckboxVisible(double timeout = 5)
    {
        ScrollToText("I confirm that I am 18 years");
        return RVisible(AgeCheckbox, timeout);
    }

    public bool IsContinueButtonVisible(double timeout = 5)
    {
        ScrollToText("Continue");
        return RVisible(ContinueText, timeout);
    }

    public bool IsSocialRegistrationTextVisible(double timeout = 5)
    {
        ScrollToText("or register with");
        return RVisible(SocialRegText, timeout);
    }

    public bool IsFacebookRegistrationVisible(double timeout = 5)
    {
        ScrollToText("or register with");
        return RVisible(FacebookButton, timeout);
    }

    public void EnterEmail(string email)
    {
        var element = RFind(EmailInput, 8);
        element.Clear();
        element.SendKeys(email);
    }

    public void EnterCreatePassword(string password)
    {
        var element = RFind(CreatePasswordInput, 8);
        element.Clear();
        element.SendKeys(password);
    }

    public void EnterConfirmPassword(string password)
    {
        var element = RFind(ConfirmPasswordInput, 8);
        element.Clear();
        element.SendKeys(password);
    }

    public void EnterReferralCode(string referralCode)
    {
        ScrollToText("Have a referral code");
        var element = RFind(ReferralInput, 8);
        element.Clear();
        element.SendKeys(referralCode);
    }

    public string GetEmailValue() => (Attr(RFind(EmailInput, 4), "text") ?? "").Trim();

    public string GetReferralValue() => (Attr(RFind(ReferralInput, 4), "text") ?? "").Trim();

    private bool IsPasswordMasked(By locator)
    {
        try
        {
            var element = RFind(locator, 4);
            var attrPassword = AttrLower(element, "password") == "true";
            var textValue = Attr(element, "text") ?? "";
            return attrPassword || textValue.Contains('\u2022');
        }
        catch
        {
            return false;
        }
    }

    public bool IsCreatePasswordMasked() => IsPasswordMasked(CreatePasswordInput);

    public bool IsConfirmPasswordMasked() => IsPasswordMasked(ConfirmPasswordInput);

    private void TogglePassword(By locator)
    {
        DismissKeyboardIfNeeded();
        try
        {
            RClickable(locator, 4).Click();
        }
        catch
        {
            RFind(locator, 4).Click();
        }
    }

    public void TapCreatePasswordToggle() => TogglePassword(CreatePasswordToggle);

    public void TapConfirmPasswordToggle() => TogglePassword(ConfirmPasswordToggle);

    private bool IsPasswordVisibleAs(By locator, string expected)
    {
        try
        {
            var element = RFind(locator, 4);
            var isVisibleAttr = AttrLower(element, "password") == "false";
            var value = (Attr(element, "text") ?? "").Trim();
            return isVisibleAttr && value == expected;
        }
        catch
        {
            return false;
        }
    }

    private bool IsPasswordHiddenFrom(By locator, string plain)
    {
        try
        {
            var element = RFind(locator, 4);
            var isMaskedAttr = AttrLower(element, "password") == "true";
            var value = (Attr(element, "text") ?? "").Trim();
            return isMaskedAttr || value != plain;
        }
        catch
        {
            return false;
        }
    }

    private static bool PollUntil(Func<bool> condition, double timeout)
    {
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed.TotalSeconds < timeout)
        {
            if (condition())
            {
                return true;
            }

            Thread.Sleep(200);
        }

        return false;
    }

    public bool WaitUntilCreatePasswordVisibleAs(string expected, double timeout = 2.5) =>
        PollUntil(() => IsPasswordVisibleAs(CreatePasswordInput, expected), timeout);

    public bool WaitUntilCreatePasswordHiddenFrom(string plain, double timeout = 2.5) =>
        PollUntil(() => IsPasswordHiddenFrom(CreatePasswordInput, plain), timeout);

    public bool WaitUntilConfirmPasswordVisibleAs(string expected, double timeout = 2.5) =>
        PollUntil(() => IsPasswordVisibleAs(ConfirmPasswordInput, expected), timeout);

    public bool WaitUntilConfirmPasswordHiddenFrom(string plain, double timeout = 2.5) =>
        PollUntil(() => IsPasswordHiddenFrom(ConfirmPasswordInput, plain), timeout);

    public void TapAgeCheckbox()
    {
        ScrollToText("I confirm that I am 18 years");

        try
        {
            Driver.FindElement(AppiumBy.AndroidUIAutomator(
                "new UiSelector().className(\"android.widget.CheckBox\").instance(0)")).Click();
            return;
        }
        catch
        {
            // fall through
        }

        AppiumElement? elem = null;
        try
        {
            RClickable(AgeCheckbox, 5).Click();
            return;
        }
        catch
        {
            try
            {
                elem = RFind(AgeCheckbox, 3);
            }
            catch
            {
                elem = null;
            }
        }

        if (elem is not null)
        {
            var bounds = ParseBounds(Attr(elem, "bounds"));
            if (bounds is not null)
            {
                var (x1, y1, x2, y2) = bounds.Value;
                TapXy((x1 + x2) / 2, (y1 + y2) / 2);
                return;
            }
        }

        var consent = RFind(ConsentText, 5);
        var consentBounds = ParseBounds(Attr(consent, "bounds"));
        if (consentBounds is null)
        {
            throw new InvalidOperationException("Could not resolve consent bounds for checkbox fallback tap");
        }

        var (cx1, cy1, _, cy2) = consentBounds.Value;
        var x = Math.Max(20, cx1 - 80);
        var y = (cy1 + cy2) / 2;
        TapXy(x, y);
    }

    public void TapAgeCheckboxByRelativeLayout()
    {
        ScrollToText("Continue");
        try
        {
            var continueElem = RFind(ContinueText, 4);
            var bounds = ParseBounds(Attr(continueElem, "bounds"));
            if (bounds is null)
            {
                return;
            }

            var (x1, y1, _, _) = bounds.Value;
            TapXy(Math.Max(40, x1 - 350), Math.Max(120, y1 - 170));
        }
        catch
        {
            // best-effort
        }
    }

    public bool IsAgeCheckboxSelected()
    {
        try
        {
            ScrollToText("I confirm that I am 18 years");
            var element = RFind(AgeCheckbox, 5);
            var checkedValue = AttrLower(element, "checked");
            if (checkedValue is "true" or "false")
            {
                return checkedValue == "true";
            }
        }
        catch
        {
            // fall through to source scan
        }

        var src = PageSource();
        var match = System.Text.RegularExpressions.Regex.Match(
            src, "class=\"android\\.widget\\.CheckBox\"[^>]*checked=\"(true|false)\"");
        if (match.Success)
        {
            return match.Groups[1].Value == "true";
        }

        return false;
    }

    public bool WaitUntilAgeCheckboxState(bool expectedSelected, double timeout = 2.5) =>
        PollUntil(() => IsAgeCheckboxSelected() == expectedSelected, timeout);

    public void SetAgeCheckboxState(bool expectedSelected, double timeout = 6.0)
    {
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed.TotalSeconds < timeout)
        {
            if (IsAgeCheckboxSelected() == expectedSelected)
            {
                return;
            }

            TapAgeCheckbox();
            Thread.Sleep(300);

            if (IsAgeCheckboxSelected() == expectedSelected)
            {
                return;
            }

            TapAgeCheckboxByRelativeLayout();
            Thread.Sleep(300);
        }

        if (expectedSelected)
        {
            TapAgeCheckbox();
        }
    }

    private AppiumElement? FindConsentElement(double timeout = 6)
    {
        var locators = new[]
        {
            ConsentText,
            AppiumBy.XPath("//android.widget.TextView[contains(@text, 'agree to the')]"),
            AppiumBy.XPath("//android.widget.TextView[contains(@text, 'Privacy Policy')]"),
            AppiumBy.XPath("//android.widget.TextView[contains(@text, 'Terms')]"),
        };

        foreach (var locator in locators)
        {
            try
            {
                return RFind(locator, timeout);
            }
            catch
            {
                // try next
            }
        }

        try
        {
            return (AppiumElement)Driver.FindElement(AppiumBy.AndroidUIAutomator(
                "new UiScrollable(new UiSelector().scrollable(true)).scrollIntoView(new UiSelector().textContains(\"Privacy Policy\"))"));
        }
        catch
        {
            // fall through
        }

        try
        {
            return (AppiumElement)Driver.FindElement(AppiumBy.AndroidUIAutomator(
                "new UiScrollable(new UiSelector().scrollable(true)).scrollIntoView(new UiSelector().textContains(\"agree to the\"))"));
        }
        catch
        {
            return null;
        }
    }

    public void ScrollToTermsAndConditionsLink() => RevealConsentFully();

    public void ScrollToPrivacyPolicyLink() => RevealConsentFully();

    private void RevealConsentFully()
    {
        if (ConsentFullyVisible())
        {
            return;
        }

        foreach (var target in new[] { "or register with", "Continue", "Privacy Policy" })
        {
            try
            {
                Driver.FindElement(AppiumBy.AndroidUIAutomator(
                    $"new UiScrollable(new UiSelector().scrollable(true)).scrollIntoView(new UiSelector().textContains(\"{target}\"))"));
                return;
            }
            catch
            {
                // try next target
            }
        }
    }

    private bool ConsentFullyVisible()
    {
        var elements = FindAll(ConsentText);
        if (elements.Count == 0)
        {
            return false;
        }

        var bounds = ParseBounds(Attr(elements[0], "bounds"));
        if (bounds is null)
        {
            return false;
        }

        var (_, y1, _, y2) = bounds.Value;
        return (y2 - y1) >= 60;
    }

    private string DetectLegalScreen(double timeout = 2.5)
    {
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed.TotalSeconds < timeout)
        {
            if (FindAll(TermsScreenTitle).Count > 0)
            {
                return "terms";
            }

            if (FindAll(PrivacyScreenTitle).Count > 0)
            {
                return "privacy";
            }

            Thread.Sleep(200);
        }

        return "";
    }

    private bool BackToRegistration(double timeout = 4)
    {
        if (!SafeBack())
        {
            return false;
        }

        return IsRegistrationScreenVisible(timeout);
    }

    public bool TapTermsAndConditionsTextSimple()
    {
        RevealConsentFully();

        var consent = FindConsentElement(4);
        if (consent is null)
        {
            return false;
        }

        var textValue = (Attr(consent, "text") ?? "").Trim();
        var bounds = ParseBounds(Attr(consent, "bounds"));
        if (bounds is null || textValue.Length == 0 || !textValue.ToLowerInvariant().Contains("terms"))
        {
            return false;
        }

        var (x1, y1, x2, y2) = bounds.Value;
        var width = x2 - x1;
        var height = y2 - y1;

        var lineCount = Math.Max(1, (int)Math.Round(height / 66.0));
        (double XRatio, double YRatio)[] termsPoints;
        if (lineCount >= 3)
        {
            termsPoints = new[] { (0.17, 0.83), (0.27, 0.83), (0.82, 0.50), (0.72, 0.50) };
        }
        else if (lineCount == 2)
        {
            termsPoints = new[] { (0.12, 0.75), (0.80, 0.25), (0.70, 0.25) };
        }
        else
        {
            termsPoints = new[] { (0.62, 0.5), (0.70, 0.5) };
        }

        foreach (var (xRatio, yRatio) in termsPoints)
        {
            var x = x1 + (int)(width * xRatio);
            var y = y1 + (int)(height * yRatio);
            TapXy(x, y);
            var screen = DetectLegalScreen(2.0);
            if (screen == "terms")
            {
                return true;
            }

            if (screen == "privacy")
            {
                BackToRegistration(4);
                RevealConsentFully();
            }
        }

        return false;
    }

    public bool TapPrivacyPolicyTextSimple()
    {
        RevealConsentFully();

        var consent = FindConsentElement(4);
        if (consent is null)
        {
            return false;
        }

        var textValue = (Attr(consent, "text") ?? "").Trim();
        var bounds = ParseBounds(Attr(consent, "bounds"));
        if (bounds is null || textValue.Length == 0 || !textValue.ToLowerInvariant().Contains("privacy policy"))
        {
            return false;
        }

        var (x1, y1, x2, y2) = bounds.Value;
        var width = x2 - x1;
        var height = y2 - y1;

        var lineCount = Math.Max(1, (int)Math.Round(height / 66.0));
        (double XRatio, double YRatio)[] privacyPoints;
        if (lineCount >= 3)
        {
            privacyPoints = new[] { (0.74, 0.83), (0.68, 0.83), (0.80, 0.83), (0.60, 0.83) };
        }
        else if (lineCount == 2)
        {
            privacyPoints = new[] { (0.74, 0.75), (0.68, 0.75), (0.82, 0.75) };
        }
        else
        {
            privacyPoints = new[] { (0.80, 0.5), (0.72, 0.5) };
        }

        foreach (var (xRatio, yRatio) in privacyPoints)
        {
            var x = x1 + (int)(width * xRatio);
            var y = y1 + (int)(height * yRatio);
            TapXy(x, y);
            var screen = DetectLegalScreen(2.0);
            if (screen == "privacy")
            {
                return true;
            }

            if (screen == "terms")
            {
                BackToRegistration(4);
                RevealConsentFully();
            }
        }

        return false;
    }

    public bool IsTermsScreenVisible(double timeout = 8) =>
        RVisible(TermsScreenTitle, timeout) && RVisible(LegalWebview, timeout);

    public bool IsPrivacyScreenVisible(double timeout = 8) =>
        RVisible(PrivacyScreenTitle, timeout) && RVisible(LegalWebview, timeout);

    public void TapContinue()
    {
        DismissKeyboardIfNeeded();
        ScrollToText("Continue");
        try
        {
            RClickable(ContinueCta, 5).Click();
        }
        catch
        {
            RClickable(ContinueText, 5).Click();
        }
    }

    public bool IsContinueButtonEnabled()
    {
        ScrollToText("Continue");
        try
        {
            var button = RFind(ContinueButton, 3);
            var buttonEnabled = AttrLower(button, "enabled") == "true";
            var buttonClickable = AttrLower(button, "clickable") == "true";
            if (buttonEnabled && buttonClickable)
            {
                return true;
            }
        }
        catch
        {
            // fall through
        }

        try
        {
            var cta = RFind(ContinueCta, 3);
            return AttrLower(cta, "enabled") == "true";
        }
        catch
        {
            return false;
        }
    }

    public void TapFacebookRegistration()
    {
        ScrollToText("or register with");
        RClickable(FacebookButton, 8).Click();
    }

    public bool IsEmailFormatErrorVisible()
    {
        RestoreRegistrationTop(3);
        if (RVisible(EmailInvalidError, 2))
        {
            return true;
        }

        var src = PageSourceLower();
        return src.Contains("invalid") && src.Contains("email");
    }

    public bool IsEmailRequiredErrorVisible()
    {
        RestoreRegistrationTop(3);
        if (RVisible(EmailRequiredError, 2))
        {
            return true;
        }

        var src = PageSourceLower();
        return src.Contains("please enter an email") || src.Contains("please enter email");
    }

    public bool IsPasswordStrengthErrorVisible()
    {
        if (RVisible(PasswordWeakError, 2) || RVisible(PasswordPolicyError, 2))
        {
            return true;
        }

        var src = PageSourceLower();
        return src.Contains("please check your password") || src.Contains("at least 8 characters");
    }

    public bool IsPasswordMismatchErrorVisible()
    {
        if (RVisible(PasswordMismatchError, 2))
        {
            return true;
        }

        return PageSourceLower().Contains("confirm password are different");
    }

    public bool IsCreatePasswordRequiredErrorVisible()
    {
        var src = PageSourceLower();
        if (src.Contains("please check your password"))
        {
            var val = (Attr(RFind(CreatePasswordInput, 3), "text") ?? "").Trim();
            return val == "";
        }

        return false;
    }

    public bool IsConfirmPasswordRequiredErrorVisible()
    {
        var src = PageSourceLower();
        if (src.Contains("confirm password") && src.Contains("different"))
        {
            return false;
        }

        try
        {
            var element = RFind(ConfirmPasswordInput, 3);
            var invalid = AttrLower(element, "contentInvalid") == "true";
            var val = (Attr(element, "text") ?? "").Trim();
            return invalid && val == "";
        }
        catch
        {
            return false;
        }
    }

    public bool IsReferralOptional()
    {
        var src = PageSourceLower();
        return src.Contains("referral") && !src.Contains("invalid referral") && !src.Contains("please enter referral");
    }

    public bool IsConsentRequiredErrorVisible()
    {
        if (RVisible(ConsentRequiredError, 2))
        {
            return true;
        }

        return PageSourceLower().Contains("please confirm you are aged 18 or older");
    }

    public bool IsTermsAndConditionsTextVisible()
    {
        RevealConsentFully();
        var consent = FindConsentElement(4);
        var text = consent is not null ? Attr(consent, "text") ?? "" : "";
        return text.Contains("Terms & Conditions") || text.Contains("Terms and Conditions");
    }

    public bool IsPrivacyPolicyTextVisible()
    {
        RevealConsentFully();
        var consent = FindConsentElement(4);
        if (consent is not null && (Attr(consent, "text") ?? "").Contains("Privacy Policy"))
        {
            return true;
        }

        return PageSource().Contains("Privacy Policy");
    }

    public bool IsAlreadyRegisteredAlertVisible() =>
        RVisible(OopsTitle, 3) && RVisible(AlreadyRegisteredMessage, 3);

    public void DismissAlreadyRegisteredAlert() => RClickable(OopsOkText, 5).Click();

    public bool IsFacebookAuthFlowDisplayed()
    {
        if (IsAlreadyRegisteredAlertVisible())
        {
            return true;
        }

        var src = PageSourceLower();
        if (src.Contains("facebook") && (src.Contains("continue") || src.Contains("log in")))
        {
            return true;
        }

        return !IsRegistrationScreenVisible(1);
    }

    public bool HasBasicAccessibilityLabels()
    {
        ScrollToText("Welcome to Shoppix");
        var checks = new List<(string Name, bool Ok)>
        {
            ("back_button", IsBackButtonVisible(3)),
            ("logo", IsLogoVisible(3)),
            ("heading", IsHeadingVisible(3)),
            ("email_label_or_placeholder", IsEmailLabelVisible(3) || IsEmailPlaceholderVisible(3)),
            ("create_password_label_or_placeholder", IsCreatePasswordLabelVisible(3) || IsCreatePasswordPlaceholderVisible(3)),
            ("confirm_password_label_or_placeholder", IsConfirmPasswordLabelVisible(3) || IsConfirmPasswordPlaceholderVisible(3)),
        };

        ScrollToText("Have a referral code");
        checks.Add(("referral_label", IsReferralLabelVisible(3)));
        checks.Add(("referral_placeholder", IsReferralPlaceholderVisible(3)));

        ScrollToText("Continue");
        checks.Add(("age_checkbox", IsAgeCheckboxVisible(3)));
        checks.Add(("terms_text", RVisible(ConsentText, 3) || IsTermsAndConditionsTextVisible()));
        checks.Add(("privacy_text", IsPrivacyPolicyTextVisible()));
        checks.Add(("continue_button", IsContinueButtonVisible(3)));
        checks.Add(("social_registration_text", IsSocialRegistrationTextVisible(3)));
        checks.Add(("facebook_registration", IsFacebookRegistrationVisible(3)));

        _lastAccessibilityMissing = checks.Where(c => !c.Ok).Select(c => c.Name).ToList();
        return _lastAccessibilityMissing.Count == 0;
    }

    public List<string> LastAccessibilityMissing() => new(_lastAccessibilityMissing);

    public bool RestoreRegistrationTop(double timeout = 5)
    {
        if (IsHeadingVisible(1.2))
        {
            return true;
        }

        try
        {
            Driver.FindElement(AppiumBy.AndroidUIAutomator(
                "new UiScrollable(new UiSelector().scrollable(true)).scrollToBeginning(10)"));
        }
        catch
        {
            // best-effort
        }

        if (IsHeadingVisible(1.8))
        {
            return true;
        }

        var rect = WindowRect(Driver);
        var left = (int)(rect.X + rect.Width * 0.08);
        var top = (int)(rect.Y + rect.Height * 0.24);
        var width = (int)(rect.Width * 0.84);
        var height = (int)(rect.Height * 0.56);

        for (var i = 0; i < 6; i++)
        {
            Driver.ExecuteScript("mobile: scrollGesture", new Dictionary<string, object>
            {
                ["left"] = left,
                ["top"] = top,
                ["width"] = width,
                ["height"] = height,
                ["direction"] = "down",
                ["percent"] = 0.9,
            });
            if (IsHeadingVisible(1.2))
            {
                return true;
            }
        }

        ScrollToText("Welcome to Shoppix");
        return IsHeadingVisible(timeout);
    }

    public bool IsCoreContentVisible()
    {
        var topOk = IsHeadingVisible(5) && IsEmailLabelVisible(5);
        var bottomOk = IsContinueButtonVisible(5);
        return topOk && bottomOk;
    }
}
