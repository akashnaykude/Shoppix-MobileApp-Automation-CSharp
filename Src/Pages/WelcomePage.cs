using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;

namespace Shoppix.MobileApp.Automation.Pages;

/// <summary>Port of src/pages/welcome_page.py.</summary>
public sealed class WelcomePage : BasePage
{
    private static readonly By Heading = AppiumBy.XPath(
        "//android.widget.TextView[@text=\"Let's get started\" or @text=\"Lets get started\" or @text=\"Let\u2019s get started\"]");

    private static readonly By Subtitle = AppiumBy.XPath(
        "//android.widget.TextView[contains(@text, 'Submit receipts') and contains(@text, 'earn rewards')]");

    private static readonly By LoginExistingUserText = AppiumBy.XPath("//android.widget.TextView[@text='Login as existing User']");

    private static readonly By LoginExistingUserCta = AppiumBy.XPath(
        "//android.widget.TextView[@text='Login as existing User']/ancestor::android.view.View[@clickable='true'][1]");

    private static readonly By OrRegisterText = AppiumBy.XPath("//android.widget.TextView[@text='or register with:']");
    private static readonly By FacebookIcon = AppiumBy.XPath("//android.widget.ImageView[@content-desc='Register with Facebook']");
    private static readonly By EmailIcon = AppiumBy.XPath("//android.widget.ImageView[@content-desc='Register with Email']");
    private static readonly By FooterVersion = AppiumBy.XPath("//android.widget.TextView[contains(@text, 'Shoppix:')]");
    private static readonly By FooterStage = AppiumBy.XPath("//android.widget.TextView[contains(@text, 'Stage')]");
    private static readonly By KantarLogo = AppiumBy.XPath("//android.widget.ImageView[@content-desc='Kantar Logo']");
    private static readonly By OopsTitle = AppiumBy.XPath("//android.widget.TextView[@text='Oops']");
    private static readonly By OopsOkCta = AppiumBy.XPath(
        "//android.widget.TextView[@text='OK']/ancestor::android.view.View[@clickable='true'][1]");
    private static readonly By OopsOkButton = AppiumBy.XPath("//android.widget.Button[ancestor::*[@pane-title='Dialogue']]");

    public WelcomePage(AndroidDriver driver) : base(driver)
    {
    }

    public bool IsWelcomeVisible(double timeout = 10) => IsVisible(Heading, timeout);

    public void TapLoginAsExistingUser() => FindClickable(LoginExistingUserCta, 12).Click();

    public void TapFacebookRegistrationIcon() => FindClickable(FacebookIcon, 12).Click();

    public void TapEmailRegistrationIcon() => FindClickable(EmailIcon, 12).Click();

    public bool IsLogoVisible() => IsVisible(KantarLogo, 8);

    public bool IsHeadingVisible() => IsVisible(Heading, 8);

    public bool IsSubtitleVisible() => IsVisible(Subtitle, 8);

    public bool IsLoginCtaVisible() => IsVisible(LoginExistingUserText, 8);

    public bool IsOrRegisterTextVisible() => IsVisible(OrRegisterText, 8);

    public bool IsFacebookIconVisible() => IsVisible(FacebookIcon, 8);

    public bool IsEmailIconVisible() => IsVisible(EmailIcon, 8);

    public bool IsFooterVersionVisible() => IsVisible(FooterVersion, 8);

    public bool IsFooterStageVisible() => IsVisible(FooterStage, 8);

    public bool IsCoreWelcomeContentVisible() =>
        IsLogoVisible()
        && IsHeadingVisible()
        && IsSubtitleVisible()
        && IsLoginCtaVisible()
        && IsOrRegisterTextVisible();

    public bool IsOopsDialogVisible(double timeout = 2) => IsVisible(OopsTitle, timeout);

    public bool DismissOopsDialogIfPresent(double timeout = 2)
    {
        if (!IsOopsDialogVisible(timeout))
        {
            return false;
        }

        try
        {
            FindClickable(OopsOkCta, 3).Click();
            return true;
        }
        catch
        {
            // fall through to button variant
        }

        try
        {
            FindClickable(OopsOkButton, 3).Click();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
