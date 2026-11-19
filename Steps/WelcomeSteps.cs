using NUnit.Framework;
using OpenQA.Selenium;
using Reqnroll;
using Shoppix.MobileApp.Automation.Pages;
using Shoppix.MobileApp.Automation.Support;
using static Shoppix.MobileApp.Automation.Support.StepHelpers;

namespace Shoppix.MobileApp.Automation.Steps;

/// <summary>Port of Steps/Welcome_steps.py (scoped to the Welcome feature).</summary>
[Binding]
[Scope(Feature = "Welcome screen")]
public sealed class WelcomeSteps
{
    private WelcomePage Welcome => new(Driver);
    private LoginPage Login => new(Driver);
    private HomePage Home => new(Driver);

    private string QuickScreenState()
    {
        string src;
        try
        {
            src = (Driver.PageSource ?? "").ToLowerInvariant();
        }
        catch
        {
            return "unknown";
        }

        if (src.Contains("let's get started") || src.Contains("lets get started") || src.Contains("let\u2019s get started"))
        {
            return "welcome";
        }

        if (src.Contains("welcome back!") || src.Contains("enter your email"))
        {
            return "login";
        }

        if (src.Contains("menu_action_setting") || src.Contains("img_header"))
        {
            return "home";
        }

        return "unknown";
    }

    private bool IsAnyKnownAppScreenVisible() =>
        Welcome.IsWelcomeVisible(4) || Login.IsLoginScreenVisible(4) || Home.IsHomeHeaderVisible(4);

    private void RecoverToWelcomeWithoutReinstall()
    {
        ActivateApp();

        if (Welcome.IsWelcomeVisible(6))
        {
            return;
        }

        var state = QuickScreenState();
        if (state == "welcome")
        {
            return;
        }

        if (state == "home")
        {
            Home.LogoutFromMenu();
            if (Welcome.IsWelcomeVisible(4))
            {
                return;
            }
        }

        if (state == "login")
        {
            if (!SafeBack())
            {
                SoftRelaunchApp();
            }

            if (Welcome.IsWelcomeVisible(4))
            {
                return;
            }
        }

        for (var i = 0; i < 2; i++)
        {
            if (Welcome.IsWelcomeVisible(3))
            {
                return;
            }

            if (Home.IsHomeHeaderVisible(3))
            {
                Home.LogoutFromMenu();
                continue;
            }

            if (Login.IsLoginScreenVisible(3))
            {
                SoftRelaunchApp();
                continue;
            }

            SoftRelaunchApp();
        }

        Assert.That(Welcome.IsWelcomeVisible(10), Is.True, "Could not navigate back to welcome screen without reinstall");
    }

    private void EnsureWelcomeScreenWithoutRelaunch()
    {
        if (QuickScreenState() == "welcome")
        {
            return;
        }

        if (Welcome.IsWelcomeVisible(2))
        {
            return;
        }

        try
        {
            RecoverToWelcomeWithoutReinstall();
            return;
        }
        catch (AssertionException)
        {
            SoftRelaunchApp();
            if (Login.IsLoginScreenVisible(3))
            {
                SafeBack();
            }
        }

        if (!Welcome.IsWelcomeVisible(6))
        {
            throw new AssertionException("Could not navigate back to welcome screen without reinstall");
        }
    }

    [Given("user is on welcome screen")]
    public void GivenUserIsOnWelcomeScreen()
    {
        EnsureWelcomeScreenWithoutRelaunch();
        Assert.That(Welcome.IsWelcomeVisible(10), Is.True, "Welcome screen was not visible");
    }

    [Then("Facebook registration button should be visible")]
    public void ThenFacebookRegistrationButtonVisible() =>
        Assert.That(Welcome.IsFacebookIconVisible(), Is.True, "Facebook registration button should be visible");

    [Then("Email registration button should be visible")]
    public void ThenEmailRegistrationButtonVisible() =>
        Assert.That(Welcome.IsEmailIconVisible(), Is.True, "Email registration button should be visible");

    [Then("app logo should be visible")]
    public void ThenAppLogoVisible() =>
        Assert.That(Welcome.IsLogoVisible(), Is.True, "App logo should be visible");

    [Then("welcome heading should be visible")]
    public void ThenWelcomeHeadingVisible() =>
        Assert.That(Welcome.IsHeadingVisible(), Is.True, "Welcome heading should be visible");

    [Then("welcome screen subtitle should be visible")]
    public void ThenWelcomeSubtitleVisible() =>
        Assert.That(Welcome.IsSubtitleVisible(), Is.True, "Welcome subtitle should be visible");

    [Then("register with text should be visible")]
    public void ThenRegisterWithTextVisible() =>
        Assert.That(Welcome.IsOrRegisterTextVisible(), Is.True, "Register with text should be visible");

    [Then("footer version text should be visible")]
    public void ThenFooterVersionVisible() =>
        Assert.That(Welcome.IsFooterVersionVisible(), Is.True, "Footer version text should be visible");

    [Then("welcome screen loads with expected UI content")]
    public void ThenWelcomeLoadsWithExpectedContent()
    {
        var page = Welcome;
        Assert.That(page.IsLogoVisible(), Is.True, "Kantar logo should be visible");
        Assert.That(page.IsHeadingVisible(), Is.True, "Welcome heading should be visible");
        Assert.That(page.IsSubtitleVisible(), Is.True, "Welcome subtitle should be visible");
        Assert.That(page.IsLoginCtaVisible(), Is.True, "Login as existing User should be visible");
        Assert.That(page.IsOrRegisterTextVisible(), Is.True, "Register helper text should be visible");
        Assert.That(page.IsFacebookIconVisible(), Is.True, "facebook icon button should be visible");
        Assert.That(page.IsEmailIconVisible(), Is.True, "email icon button should be visible");
        Assert.That(page.IsFooterVersionVisible(), Is.True, "version text should be visible");
        Assert.That(page.IsFooterStageVisible(), Is.True, "environment label should be visible");
    }

    [When("user taps Login as existing User")]
    public void WhenTapLoginAsExistingUser() => Welcome.TapLoginAsExistingUser();

    [When("user taps Facebook registration icon")]
    public void WhenTapFacebookRegistrationIcon() => Welcome.TapFacebookRegistrationIcon();

    [When("user taps Email registration icon")]
    public void WhenTapEmailRegistrationIcon() => Welcome.TapEmailRegistrationIcon();

    [Then("login screen should be displayed")]
    public void ThenLoginScreenDisplayed() =>
        Assert.That(Login.IsLoginScreenVisible(12), Is.True, "Login screen was not visible");

    [Then("Facebook login flow should be displayed")]
    public void ThenFacebookLoginFlowDisplayed()
    {
        var page = Welcome;
        if (page.DismissOopsDialogIfPresent(5))
        {
            return;
        }

        if (QuickScreenState() == "welcome")
        {
            Assert.That(page.IsWelcomeVisible(1), Is.False, "Facebook login flow did not open");
        }

        try
        {
            Driver.Navigate().Back();
        }
        catch
        {
            // keep stable
        }
    }

    [Then("email registration flow should be displayed")]
    public void ThenEmailRegistrationFlowDisplayed()
    {
        if (QuickScreenState() == "welcome")
        {
            Assert.That(Welcome.IsWelcomeVisible(1), Is.False, "Email registration flow did not open");
        }
    }

    [When("user cancels Facebook login")]
    public void WhenCancelFacebookLogin() => SafeBack();

    [When("user cancels email registration")]
    public void WhenCancelEmailRegistration() => SafeBack();

    [Then("user should return to welcome screen")]
    public void ThenUserReturnsToWelcome()
    {
        EnsureWelcomeScreenWithoutRelaunch();
        Assert.That(Welcome.IsWelcomeVisible(10), Is.True, "User did not return to welcome screen");
    }

    [When("app is sent to background")]
    public void WhenAppSentToBackground() => BackgroundApp(2);

    [When("app is brought to foreground")]
    public void WhenAppBroughtToForeground()
    {
        Assert.That(string.IsNullOrEmpty(AppPackage), Is.False, "App package not available in capabilities");
        ActivateApp();
    }

    [When("device orientation is changed")]
    public void WhenDeviceOrientationChanged() => ChangeOrientation();

    [Then("welcome screen should be displayed correctly")]
    public void ThenWelcomeDisplayedCorrectly()
    {
        var page = Welcome;
        Assert.That(page.IsWelcomeVisible(10), Is.True, "Welcome screen heading was not visible");
        Assert.That(page.IsCoreWelcomeContentVisible(), Is.True, "Welcome screen content was not displayed correctly");
    }

    [When("user presses Android back")]
    public void WhenPressAndroidBack()
    {
        try
        {
            Driver.Navigate().Back();
        }
        catch
        {
            // non-blocking
        }
    }

    [Then("app should handle back press gracefully")]
    public void ThenBackPressHandledGracefully()
    {
        ActivateApp();
        Assert.That(IsAnyKnownAppScreenVisible(), Is.True, "App did not recover to a known screen after back press");
    }

    [When("user taps Login as existing User twice quickly")]
    public void WhenTapLoginAsExistingUserTwice()
    {
        Welcome.TapLoginAsExistingUser();
        if (Welcome.IsWelcomeVisible(2))
        {
            Welcome.TapLoginAsExistingUser();
        }
        else if (Login.IsLoginScreenVisible(3))
        {
            // already navigated
        }
    }

    [When("user presses Android back from login screen")]
    public void WhenPressAndroidBackFromLogin() => SafeBack();

    [Then("welcome screen should be displayed again")]
    public void ThenWelcomeDisplayedAgain() =>
        Assert.That(Welcome.IsWelcomeVisible(10), Is.True, "Welcome screen was not visible after back");

    [Then("welcome screen should be displayed")]
    public void ThenWelcomeDisplayed() =>
        Assert.That(Welcome.IsWelcomeVisible(10), Is.True, "Welcome screen was not visible");
}
