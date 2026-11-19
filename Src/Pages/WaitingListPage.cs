using System.Diagnostics;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;

namespace Shoppix.MobileApp.Automation.Pages;

/// <summary>Port of src/pages/waiting_list_page.py.</summary>
public sealed class WaitingListPage : BasePage
{
    private static readonly By BackButton = AppiumBy.XPath("//android.widget.ImageView[@content-desc='Back']");
    private static readonly By Logo = AppiumBy.XPath("//android.widget.ImageView[@content-desc='Kantar Logo']");
    private static readonly By Heading = AppiumBy.XPath("//android.widget.TextView[@text='Thank you for your interest in Shoppix!']");
    private static readonly By DescriptionText = AppiumBy.XPath("//android.widget.TextView[contains(@text, 'high demand')]");
    private static readonly By ReferralInstruction = AppiumBy.XPath("//android.widget.TextView[contains(@text, 'Have a valid referral code')]");

    private static readonly By EmailLabel = AppiumBy.XPath("//android.widget.TextView[@text='Email']");
    private static readonly By PostcodeLabel = AppiumBy.XPath("//android.widget.TextView[@text='Postcode']");
    private static readonly By AgeLabel = AppiumBy.XPath("//android.widget.TextView[@text='Age range']");
    private static readonly By ReferralLabel = AppiumBy.XPath("//android.widget.TextView[@text='Have a referral code?']");

    private static readonly By EmailInput = AppiumBy.AndroidUIAutomator("new UiSelector().className(\"android.widget.EditText\").instance(0)");
    private static readonly By PostcodeInput = AppiumBy.AndroidUIAutomator("new UiSelector().className(\"android.widget.EditText\").instance(1)");
    private static readonly By ReferralInput = AppiumBy.AndroidUIAutomator("new UiSelector().className(\"android.widget.EditText\").instance(3)");

    private static readonly By EmailPlaceholder = AppiumBy.XPath("//android.widget.TextView[@text='Enter your email']");
    private static readonly By PostcodePlaceholder = AppiumBy.XPath("//android.widget.TextView[@text='Enter your postcode']");
    private static readonly By AgePlaceholder = AppiumBy.XPath("//android.widget.TextView[@text='Select your age']");
    private static readonly By ReferralPlaceholder = AppiumBy.XPath("//android.widget.TextView[@text='Enter referral code']");

    private static readonly By AgeDropdown = AppiumBy.XPath(
        "//android.view.View[@clickable='true' and .//android.widget.EditText[@enabled='false']]");
    private static readonly By AgeValue = AppiumBy.AndroidUIAutomator("new UiSelector().className(\"android.widget.EditText\").instance(2)");

    private static readonly string[] AgeRangeOptions = { "18-24", "25-34", "35-44", "45-54", "55-64", "65+" };

    private static readonly By ConsentCheckbox = AppiumBy.XPath("//android.widget.CheckBox");
    private static readonly By ConsentText = AppiumBy.XPath("//android.widget.TextView[contains(@text, 'consent to Worldpanel')]");
    private static readonly By PrivacyNote = AppiumBy.XPath(
        "//android.widget.TextView[contains(@text, 'personal information will only be used')]");

    private static readonly By ContinueText = AppiumBy.XPath("//android.widget.TextView[@text='Continue']");
    private static readonly By ContinueCta = AppiumBy.XPath(
        "//android.widget.TextView[@text='Continue']/ancestor::android.view.View[@clickable='true'][1]");

    private static readonly By EmailRequiredError = AppiumBy.XPath("//android.widget.TextView[@text='Please enter an email address']");
    private static readonly By EmailInvalidError = AppiumBy.XPath("//android.widget.TextView[@text='Please enter a valid email address']");
    private static readonly By PostcodeRequiredError = AppiumBy.XPath("//android.widget.TextView[@text='Please enter a postcode']");
    private static readonly By PostcodeInvalidError = AppiumBy.XPath("//android.widget.TextView[@text='Please enter a valid postcode']");
    private static readonly By AgeRequiredError = AppiumBy.XPath("//android.widget.TextView[@text='Please select age']");
    private static readonly By ConsentRequiredError = AppiumBy.XPath(
        "//android.widget.TextView[@text='Please confirm that you are over 18 years old']");

    private static readonly By SuccessAlertMessage = AppiumBy.XPath("//android.widget.TextView[contains(@text, 'registered successfully')]");
    private static readonly By DoneText = AppiumBy.XPath("//android.widget.TextView[@text='Done']");
    private static readonly By DoneCta = AppiumBy.XPath(
        "//android.widget.TextView[@text='Done']/ancestor::android.view.View[@clickable='true'][1]");

    private static readonly By OopsTitle = AppiumBy.XPath("//android.widget.TextView[@text='Oops']");
    private static readonly By DuplicateAlertMessage = AppiumBy.XPath(
        "//android.widget.TextView[@text='This email address is already on the waitlist.']");
    private static readonly By OkText = AppiumBy.XPath("//android.widget.TextView[@text='OK']");
    private static readonly By OkCta = AppiumBy.XPath(
        "//android.widget.TextView[@text='OK']/ancestor::android.view.View[@clickable='true'][1]");

    public WaitingListPage(AndroidDriver driver) : base(driver)
    {
    }

    private AppiumElement WFind(By locator, double timeout = 10) => Find(locator, timeout);

    private AppiumElement WClickable(By locator, double timeout = 15) => FindClickable(locator, timeout);

    private void ScrollToTop()
    {
        try
        {
            Driver.FindElement(AppiumBy.AndroidUIAutomator(
                "new UiScrollable(new UiSelector().scrollable(true)).flingToBeginning(8)"));
        }
        catch
        {
            // best-effort
        }
    }

    private bool Shown(By locator, string? scrollHint = null, double timeout = 4)
    {
        if (Present(locator))
        {
            return true;
        }

        if (scrollHint is not null)
        {
            ScrollToText(scrollHint);
            if (Present(locator))
            {
                return true;
            }
        }

        return IsVisible(locator, timeout);
    }

    public void NavigateFromWelcome()
    {
        var welcome = new WelcomePage(Driver);
        if (welcome.IsWelcomeVisible(3))
        {
            welcome.TapEmailRegistrationIcon();
        }
    }

    public bool IsWaitingListVisible(double timeout = 6)
    {
        if (Present(PostcodeLabel))
        {
            return true;
        }

        return IsVisible(PostcodeLabel, timeout);
    }

    public void ResetForm()
    {
        foreach (var locator in new[] { EmailInput, PostcodeInput, ReferralInput })
        {
            try
            {
                WFind(locator, 2).Clear();
            }
            catch
            {
                // best-effort
            }
        }
    }

    public bool IsBackButtonVisible(double timeout = 4) => Shown(BackButton, null, timeout);

    public bool IsLogoVisible(double timeout = 4)
    {
        ScrollToTop();
        return Shown(Logo, "Thank you for your interest", timeout);
    }

    public bool IsHeadingVisible(double timeout = 4)
    {
        ScrollToTop();
        return Shown(Heading, "Thank you for your interest", timeout);
    }

    public bool IsDescriptionTextVisible(double timeout = 4)
    {
        ScrollToTop();
        if (Shown(DescriptionText, "high demand", timeout))
        {
            return true;
        }

        var src = PageSourceLower();
        return src.Contains("high demand")
               || src.Contains("waiting list for new shoppix sign-ups")
               || src.Contains("contact you when spaces become available");
    }

    public bool IsReferralInstructionVisible(double timeout = 4)
    {
        ScrollToTop();
        if (Shown(ReferralInstruction, "referral code", timeout))
        {
            return true;
        }

        var src = PageSourceLower();
        return src.Contains("have a valid referral code")
               || (src.Contains("referral code") && src.Contains("join shoppix today"));
    }

    public bool IsEmailLabelVisible(double timeout = 4) => Shown(EmailLabel, null, timeout);

    public bool IsEmailPlaceholderVisible(double timeout = 4) => Shown(EmailPlaceholder, null, timeout);

    public bool IsPostcodeLabelVisible(double timeout = 4) => Shown(PostcodeLabel, "Postcode", timeout);

    public bool IsPostcodePlaceholderVisible(double timeout = 4) => Shown(PostcodePlaceholder, "Enter your postcode", timeout);

    public bool IsAgeLabelVisible(double timeout = 4) => Shown(AgeLabel, "Age range", timeout);

    public bool IsAgeDropdownVisible(double timeout = 4)
    {
        if (Present(AgePlaceholder) || Present(AgeDropdown))
        {
            return true;
        }

        ScrollToText("Age range");
        return Present(AgePlaceholder) || Present(AgeDropdown);
    }

    public bool IsReferralLabelVisible(double timeout = 4) => Shown(ReferralLabel, "Have a referral code?", timeout);

    public bool IsReferralPlaceholderVisible(double timeout = 4) => Shown(ReferralPlaceholder, "Enter referral code", timeout);

    public bool IsConsentCheckboxVisible(double timeout = 4) => Shown(ConsentCheckbox, "consent to Worldpanel", timeout);

    public bool IsConsentTextVisible(double timeout = 4) => Shown(ConsentText, "consent to Worldpanel", timeout);

    public bool IsPrivacyNoteVisible(double timeout = 4) => Shown(PrivacyNote, "personal information", timeout);

    public bool IsContinueButtonVisible(double timeout = 4) => Shown(ContinueText, "Continue", timeout);

    public void EnterEmail(string email)
    {
        var element = WFind(EmailInput, 8);
        element.Clear();
        element.SendKeys(email);
        DismissKeyboardIfNeeded();
    }

    public void EnterPostcode(string postcode)
    {
        var element = WFind(PostcodeInput, 8);
        element.Clear();
        element.SendKeys(postcode);
        DismissKeyboardIfNeeded();
    }

    public void EnterReferralCode(string referralCode)
    {
        ScrollToText("Have a referral code?");
        var element = WFind(ReferralInput, 8);
        element.Clear();
        element.SendKeys(referralCode);
        DismissKeyboardIfNeeded();
    }

    public string GetEmailValue() => (Attr(WFind(EmailInput, 4), "text") ?? "").Trim();

    public string GetPostcodeValue() => (Attr(WFind(PostcodeInput, 4), "text") ?? "").Trim();

    public string GetReferralValue() => (Attr(WFind(ReferralInput, 4), "text") ?? "").Trim();

    public void OpenAgeDropdown()
    {
        DismissKeyboardIfNeeded();
        if (!Present(AgeDropdown) && !Present(AgePlaceholder))
        {
            ScrollToText("Age range");
        }

        try
        {
            WClickable(AgeDropdown, 3).Click();
            return;
        }
        catch
        {
            // fall through
        }

        try
        {
            TapElementCenter(WFind(AgeDropdown, 3));
        }
        catch
        {
            TapElementCenter(WFind(AgePlaceholder, 3));
        }
    }

    private static By AgeOptionLocator(string option) => AppiumBy.XPath($"//android.widget.TextView[@text='{option}']");

    public bool IsAgeOptionsDisplayed(double timeout = 4)
    {
        if (Present(AgeOptionLocator("18-24")))
        {
            return true;
        }

        return IsVisible(AgeOptionLocator("18-24"), timeout);
    }

    public bool AreAllAgeOptionsVisible() => AgeRangeOptions.All(option => Present(AgeOptionLocator(option)));

    public void SelectAgeRange(string option)
    {
        var locator = AgeOptionLocator(option);
        if (!Present(locator))
        {
            OpenAgeDropdown();
        }

        try
        {
            WClickable(locator, 3).Click();
        }
        catch
        {
            TapElementCenter(WFind(locator, 3));
        }
    }

    public string GetSelectedAge()
    {
        string value;
        try
        {
            value = (Attr(WFind(AgeValue, 3), "text") ?? "").Trim();
        }
        catch
        {
            value = "";
        }

        if (value.Length > 0 && value != "Select your age")
        {
            return value;
        }

        return "";
    }

    public bool IsConsentChecked()
    {
        try
        {
            if (!Present(ConsentCheckbox))
            {
                ScrollToText("consent to Worldpanel");
            }

            var element = WFind(ConsentCheckbox, 4);
            var checkedValue = AttrLower(element, "checked");
            if (checkedValue is "true" or "false")
            {
                return checkedValue == "true";
            }
        }
        catch
        {
            // fall through
        }

        var src = PageSource();
        var match = System.Text.RegularExpressions.Regex.Match(
            src, "class=\"android\\.widget\\.CheckBox\"[^>]*checked=\"(true|false)\"");
        return match.Success && match.Groups[1].Value == "true";
    }

    public void TapConsentCheckbox()
    {
        ScrollToText("consent to Worldpanel");
        try
        {
            WClickable(ConsentCheckbox, 5).Click();
        }
        catch
        {
            TapElementCenter(WFind(ConsentCheckbox, 4));
        }
    }

    public void SetConsent(bool expectedChecked, double timeout = 5.0)
    {
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed.TotalSeconds < timeout)
        {
            if (IsConsentChecked() == expectedChecked)
            {
                return;
            }

            TapConsentCheckbox();
            Thread.Sleep(300);
        }
    }

    public void TapContinue()
    {
        ScrollToText("Continue");
        try
        {
            WClickable(ContinueCta, 6).Click();
        }
        catch
        {
            TapElementCenter(WFind(ContinueText, 5));
        }
    }

    public void TapContinueTwice()
    {
        ScrollToText("Continue");
        var element = WClickable(ContinueCta, 6);
        element.Click();
        try
        {
            element.Click();
        }
        catch
        {
            // second tap is best-effort
        }
    }

    public void FillValidForm(string email, string postcode, string age = "25-34")
    {
        EnterEmail(email);
        EnterPostcode(postcode);
        OpenAgeDropdown();
        SelectAgeRange(age);
        SetConsent(true);
    }

    public bool IsEmailRequiredErrorVisible(double timeout = 4) => Shown(EmailRequiredError, "Please enter an email address", timeout);

    public bool IsEmailInvalidErrorVisible(double timeout = 4) => Shown(EmailInvalidError, "Please enter a valid email address", timeout);

    public bool IsPostcodeRequiredErrorVisible(double timeout = 4) => Shown(PostcodeRequiredError, "Please enter a postcode", timeout);

    public bool IsPostcodeInvalidErrorVisible(double timeout = 4) => Shown(PostcodeInvalidError, "Please enter a valid postcode", timeout);

    public bool IsPostcodeInvalidErrorPresent() => Present(PostcodeInvalidError);

    public bool IsAgeRequiredErrorVisible(double timeout = 4) => Shown(AgeRequiredError, "Please select age", timeout);

    public bool IsConsentRequiredErrorVisible(double timeout = 4) => Shown(ConsentRequiredError, "confirm that you are over 18", timeout);

    public bool IsAnyErrorOrOopsVisible(double timeout = 6)
    {
        if (IsVisible(OopsTitle, timeout))
        {
            return true;
        }

        foreach (var locator in new[] { EmailInvalidError, PostcodeInvalidError, EmailRequiredError, PostcodeRequiredError })
        {
            if (Present(locator))
            {
                return true;
            }
        }

        return false;
    }

    public bool IsSuccessAlertVisible(double timeout = 25) => IsVisible(SuccessAlertMessage, timeout);

    public bool IsDoneButtonVisible(double timeout = 6) => IsVisible(DoneText, timeout);

    public void TapDone()
    {
        try
        {
            WClickable(DoneCta, 6).Click();
        }
        catch
        {
            TapElementCenter(WFind(DoneText, 5));
        }
    }

    public bool IsDuplicateAlertVisible(double timeout = 20) => IsVisible(DuplicateAlertMessage, timeout);

    public string GetDuplicateAlertText()
    {
        try
        {
            return (Attr(WFind(DuplicateAlertMessage, 8), "text") ?? "").Trim();
        }
        catch
        {
            return "";
        }
    }

    public int DuplicateAlertCount() => FindAll(DuplicateAlertMessage).Count;

    public bool IsOkButtonVisible(double timeout = 6) => IsVisible(OkText, timeout);

    public void TapOk()
    {
        try
        {
            WClickable(OkCta, 6).Click();
        }
        catch
        {
            TapElementCenter(WFind(OkText, 5));
        }
    }

    public bool IsAlertDismissed(double timeout = 6)
    {
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed.TotalSeconds < timeout)
        {
            if (!Present(DuplicateAlertMessage) && !Present(OopsTitle))
            {
                return true;
            }

            Thread.Sleep(300);
        }

        return false;
    }

    public Dictionary<string, bool> KeyElementsAccessibilityStatus()
    {
        string backDesc;
        try
        {
            backDesc = Attr(WFind(BackButton, 5), "content-desc") ?? "";
        }
        catch
        {
            backDesc = "";
        }

        var backOk = backDesc.Trim().Length > 0;

        ScrollToText("Thank you for your interest");
        var logoPresent = Present(Logo);
        var logoOk = true;
        if (logoPresent)
        {
            try
            {
                var logoDesc = Attr(WFind(Logo, 3), "content-desc") ?? "";
                logoOk = logoDesc.Trim().Length > 0;
            }
            catch
            {
                logoOk = false;
            }
        }

        var src = PageSourceLower();
        var labelsPresent = new[] { "email", "postcode", "age range" }.All(src.Contains);

        return new Dictionary<string, bool>
        {
            ["back_ok"] = backOk,
            ["logo_ok"] = logoOk,
            ["labels_present"] = labelsPresent,
        };
    }

    public bool KeyElementsHaveAccessibilityLabels() => KeyElementsAccessibilityStatus().Values.All(v => v);

    public bool CoreContentRendered()
    {
        if (!(IsEmailLabelVisible(5) && IsPostcodeLabelVisible(5) && IsAgeLabelVisible(5)))
        {
            return false;
        }

        ScrollToText("Continue");
        try
        {
            var element = WFind(ContinueText, 5);
            var bounds = ParseBounds(Attr(element, "bounds"));
            if (bounds is not null)
            {
                var (_, _, x2, _) = bounds.Value;
                var width = Driver.Manage().Window.Size.Width;
                if (x2 > width + 5)
                {
                    return false;
                }
            }
        }
        catch
        {
            return false;
        }

        return true;
    }
}
