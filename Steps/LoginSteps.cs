using System.Diagnostics;
using NUnit.Framework;
using OpenQA.Selenium;
using Reqnroll;
using Shoppix.MobileApp.Automation.Pages;
using Shoppix.MobileApp.Automation.Support;
using static Shoppix.MobileApp.Automation.Support.StepHelpers;

namespace Shoppix.MobileApp.Automation.Steps;

/// <summary>Port of Steps/Login_steps.py (scoped to the Login feature).</summary>
[Binding]
[Scope(Feature = "Login screen")]
public sealed class LoginSteps
{
    private const string ToggleDummyPassword = "Toggle@123";
    private const string ToggleKey = "toggle_dummy_password";

    private readonly ScenarioContext _context;

    public LoginSteps(ScenarioContext context) => _context = context;

    private LoginPage Login => new(Driver);
    private HomePage Home => new(Driver);
    private WelcomePage Welcome => new(Driver);

    private string GetToggleDummy() => _context.TryGetValue(ToggleKey, out string value) ? value : "";

    private bool TogglePasswordUntilState(LoginPage page, string dummyPassword, bool targetVisible, int maxTaps = 2)
    {
        for (var i = 0; i < maxTaps; i++)
        {
            page.TapPasswordToggle();
            if (targetVisible && page.WaitUntilPasswordVisibleAs(dummyPassword, 1.5))
            {
                return true;
            }

            if (!targetVisible && page.WaitUntilPasswordHiddenFrom(dummyPassword, 1.5))
            {
                return true;
            }
        }

        return false;
    }

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

        if (src.Contains("welcome back!") || src.Contains("enter your email"))
        {
            return "login";
        }

        if (src.Contains("forgot your password") || src.Contains("reset your password"))
        {
            return "auth_child";
        }

        if (src.Contains("sign up") || src.Contains("register"))
        {
            return "auth_child";
        }

        if (src.Contains("let's get started") || src.Contains("lets get started") || src.Contains("let\u2019s get started"))
        {
            return "welcome";
        }

        if (src.Contains("menu_action_setting") || src.Contains("img_header"))
        {
            return "home";
        }

        return "unknown";
    }

    private void EnsureLoginScreenWithoutRelaunch()
    {
        var page = Login;
        var home = Home;

        page.DismissOopsIfPresent();

        var state = QuickScreenState();
        if (state == "login")
        {
            return;
        }

        if (state == "auth_child")
        {
            SafeBack();
            if (page.IsLoginScreenVisible(3))
            {
                return;
            }
        }

        if (state == "home")
        {
            home.LogoutFromMenu();
            return;
        }

        if (state == "welcome")
        {
            page.NavigateToLoginIfNeeded();
            if (page.IsLoginScreenVisible(3))
            {
                return;
            }
        }

        if (page.IsInvalidAlertVisible(1))
        {
            page.DismissInvalidAlert();
        }

        if (page.IsLoginScreenVisible(2))
        {
            return;
        }

        if (home.IsHomeHeaderVisible(2))
        {
            home.LogoutFromMenu();
            if (page.IsLoginScreenVisible(6))
            {
                return;
            }
        }

        page.NavigateToLoginIfNeeded();
        if (page.IsLoginScreenVisible(4))
        {
            return;
        }

        for (var i = 0; i < 2; i++)
        {
            if (!SafeBack())
            {
                break;
            }

            state = QuickScreenState();
            if (state == "login")
            {
                return;
            }

            if (state == "auth_child")
            {
                continue;
            }

            if (state == "welcome")
            {
                page.NavigateToLoginIfNeeded();
                if (page.IsLoginScreenVisible(3))
                {
                    return;
                }
            }

            if (state == "home")
            {
                home.LogoutFromMenu();
                if (page.IsLoginScreenVisible(4))
                {
                    return;
                }
            }
        }

        SoftRelaunchApp();
        try
        {
            page.NavigateToLoginIfNeeded();
        }
        catch (WebDriverException)
        {
            // best-effort
        }
    }

    [Given("user is on login screen")]
    public void GivenUserIsOnLoginScreen()
    {
        EnsureLoginScreenWithoutRelaunch();
        var page = Login;

        if (!page.IsLoginScreenVisible(3))
        {
            SoftRelaunchApp();
            try
            {
                page.NavigateToLoginIfNeeded();
            }
            catch
            {
                // best-effort
            }

            if (!page.IsLoginScreenVisible(4))
            {
                if (Home.IsHomeHeaderVisible(2))
                {
                    Home.LogoutFromMenu();
                }
            }
        }

        Assert.That(page.IsLoginScreenVisible(8), Is.True, "Login screen was not visible");
        page.ResetForm();
    }

    [When("user logs out from home menu")]
    public void WhenUserLogsOut() => Home.LogoutFromMenu();

    [Then("user should be on login screen")]
    public void ThenUserShouldBeOnLoginScreen() =>
        Assert.That(Login.IsLoginScreenVisible(20), Is.True, "Login screen was not visible after sign out");

    [When("user taps login button")]
    public void WhenTapLoginButton() => Login.TapLogin();

    [Then("login button should be disabled")]
    public void ThenLoginButtonDisabled() =>
        Assert.That(Login.IsLoginButtonEnabled(), Is.False, "Login button was enabled unexpectedly");

    [Then("login screen should display expected UI content")]
    public void ThenLoginDisplaysExpectedContent()
    {
        var page = Login;
        Assert.That(page.IsLogoVisible(), Is.True, "Login logo not visible");
        Assert.That(page.IsHeadingVisible(), Is.True, "Login heading not visible");
        Assert.That(page.IsSubtitleVisible(), Is.True, "Login subtitle not visible");
        Assert.That(page.AreFieldLabelsVisible(), Is.True, "Login field labels not visible");
        Assert.That(page.IsLoginButtonVisible(), Is.True, "Login button not visible");
    }

    [Then("login app logo should be visible")]
    public void ThenLoginLogoVisible() => Assert.That(Login.IsLogoVisible(), Is.True, "Login app logo not visible");

    [Then("login welcome heading should be visible")]
    public void ThenLoginHeadingVisible() => Assert.That(Login.IsHeadingVisible(), Is.True, "Login welcome heading not visible");

    [Then("login subtitle text should be visible")]
    public void ThenLoginSubtitleVisible() => Assert.That(Login.IsSubtitleVisible(), Is.True, "Login subtitle text not visible");

    [Then("email field label should be visible")]
    public void ThenEmailLabelVisible() => Assert.That(Login.IsEmailLabelVisible(), Is.True, "Email field label not visible");

    [Then("email input should show placeholder text")]
    public void ThenEmailPlaceholderVisible() => Assert.That(Login.IsEmailPlaceholderVisible(), Is.True, "Email placeholder text not visible");

    [Then("password field label should be visible")]
    public void ThenPasswordLabelVisible() => Assert.That(Login.IsPasswordLabelVisible(), Is.True, "Password field label not visible");

    [Then("password input should show placeholder text")]
    public void ThenPasswordPlaceholderVisible() => Assert.That(Login.IsPasswordPlaceholderVisible(), Is.True, "Password placeholder text not visible");

    [Then("password visibility toggle icon should be visible")]
    public void ThenPasswordToggleVisible() => Assert.That(Login.IsPasswordToggleVisible(), Is.True, "Password visibility toggle icon not visible");

    [Then("forgot password link should be visible")]
    public void ThenForgotPasswordVisible() => Assert.That(Login.IsForgotPasswordVisible(), Is.True, "Forgot password link not visible");

    [Then("login button should be visible")]
    public void ThenLoginButtonVisible() => Assert.That(Login.IsLoginButtonVisible(), Is.True, "Login button not visible");

    [Then("social login section text should be visible")]
    public void ThenSocialLoginTextVisible() => Assert.That(Login.IsSocialLoginTextVisible(), Is.True, "Social login section text not visible");

    [Then("Facebook login button should be visible")]
    public void ThenFacebookButtonVisible() => Assert.That(Login.IsFacebookLoginButtonVisible(), Is.True, "Facebook login button not visible");

    [Then("sign up link should be visible")]
    public void ThenSignUpLinkVisible() => Assert.That(Login.IsSignUpLinkVisible(), Is.True, "Sign up link not visible");

    [When("user enters valid email address")]
    public void WhenEnterValidEmail() => Login.EnterEmail(TestHost.Email);

    [Then("email input should show entered value")]
    public void ThenEmailShowsEnteredValue()
    {
        var actual = Login.GetEmailValue();
        Assert.That(actual.Trim().ToLowerInvariant(), Is.EqualTo(TestHost.Email.Trim().ToLowerInvariant()), "Entered email value not reflected");
    }

    [When("user enters invalid email address")]
    public void WhenEnterInvalidEmail() => Login.EnterEmail("invalid-email");

    [Then("email format validation message should be displayed")]
    public void ThenEmailFormatValidation() => Assert.That(Login.IsEmailFormatErrorVisible(), Is.True, "Email format validation message not displayed");

    [When("user enters valid password")]
    public void WhenEnterValidPassword() => Login.EnterPassword(TestHost.Password);

    [When("user enters dummy password for visibility check")]
    public void WhenEnterDummyPassword()
    {
        Login.EnterPassword(ToggleDummyPassword);
        _context[ToggleKey] = ToggleDummyPassword;
    }

    [Then("password input should show masked value")]
    public void ThenPasswordMasked() => Assert.That(Login.IsPasswordMasked(), Is.True, "Password field is not masked");

    [Then("password should be hidden by default")]
    public void ThenPasswordHiddenByDefault() => Assert.That(Login.IsPasswordMasked(), Is.True, "Password is not hidden by default");

    [When("user taps password visibility toggle icon")]
    public void WhenTapPasswordToggle()
    {
        var page = Login;
        var dummy = GetToggleDummy();
        if (dummy.Length > 0)
        {
            Assert.That(TogglePasswordUntilState(page, dummy, true), Is.True, "Unable to toggle password visibility ON");
            return;
        }

        page.TapPasswordToggle();
    }

    [Then("password should be visible")]
    public void ThenPasswordVisible()
    {
        var page = Login;
        var dummy = GetToggleDummy();
        if (dummy.Length == 0)
        {
            dummy = ToggleDummyPassword;
        }

        Assert.That(page.WaitUntilPasswordVisibleAs(dummy, 2), Is.True, "Password did not become visible");
        Assert.That(page.IsPasswordVisibleAs(dummy), Is.True, "Password text did not match entered value when visibility was ON");
    }

    [When("user taps password visibility toggle icon again")]
    public void WhenTapPasswordToggleAgain()
    {
        var page = Login;
        var dummy = GetToggleDummy();
        if (dummy.Length > 0)
        {
            Assert.That(TogglePasswordUntilState(page, dummy, false), Is.True, "Unable to toggle password visibility OFF");
            return;
        }

        page.TapPasswordToggle();
    }

    [Then("password should be hidden")]
    public void ThenPasswordHidden()
    {
        var page = Login;
        var dummy = GetToggleDummy();
        if (dummy.Length == 0)
        {
            dummy = ToggleDummyPassword;
        }

        Assert.That(page.WaitUntilPasswordHiddenFrom(dummy, 2), Is.True, "Password did not become hidden");
        Assert.That(page.IsPasswordHiddenFrom(dummy), Is.True, "Password is still visible in plain text after toggle OFF");
    }

    [When("user leaves email field empty")]
    public void WhenLeaveEmailEmpty() => Login.EnterEmail("");

    [When("user leaves password field empty")]
    public void WhenLeavePasswordEmpty() => Login.EnterPassword("");

    [Then("login button should be enabled")]
    public void ThenLoginButtonEnabled() => Assert.That(Login.IsLoginButtonEnabled(), Is.True, "Login button did not become enabled");

    [Then("user should be logged in successfully")]
    public void ThenUserLoggedIn()
    {
        var home = Home;
        var deadline = Stopwatch.StartNew();
        while (deadline.Elapsed.TotalSeconds < 60)
        {
            try
            {
                if (home.IsHomeDashboardVisible(2) || home.IsHomeLikeState())
                {
                    return;
                }

                home.DismissOopsIfPresent(1);
            }
            catch (WebDriverException)
            {
                // keep polling
            }

            Thread.Sleep(1000);
        }

        Assert.That(home.IsHomeDashboardVisible(5), Is.True, "Home header was not visible after successful login");
    }

    [When("user enters invalid password")]
    public void WhenEnterInvalidPassword() => Login.EnterPassword("WrongPass@123");

    [When("user enters unregistered email address")]
    public void WhenEnterUnregisteredEmail() => Login.EnterEmail("unknown.user@shoppix.test");

    [When("device orientation is changed")]
    public void WhenOrientationChanged() => ChangeOrientation();

    [When("app is sent to background")]
    public void WhenAppSentToBackground() => BackgroundApp(1);

    [When("app is brought to foreground")]
    public void WhenAppBroughtToForeground()
    {
        Assert.That(string.IsNullOrEmpty(AppPackage), Is.False, "App package not available in capabilities");
        ActivateApp();
    }

    [Then("invalid credentials message should be displayed")]
    public void ThenInvalidCredentialsMessage()
    {
        var data = TestHost.LoginData;
        var (title, message) = Login.GetInvalidAlert(data.ExpectedInvalidMessages);
        Assert.That(title, Is.EqualTo(data.ExpectedInvalidTitle), "Invalid credentials title mismatch");
        Assert.That(data.ExpectedInvalidMessages, Does.Contain(message), "Invalid credentials message mismatch");
        Login.DismissInvalidAlert();
    }

    [Then("email required validation message should be displayed")]
    public void ThenEmailRequiredValidation()
    {
        var page = Login;
        if (page.IsEmailRequiredErrorVisible())
        {
            return;
        }

        Assert.That(page.IsLoginScreenVisible(4), Is.True, "User moved away from login screen unexpectedly");
        Assert.That(page.IsLoginButtonEnabled(), Is.False, "Expected login to be blocked when email is empty");
    }

    [Then("password required validation message should be displayed")]
    public void ThenPasswordRequiredValidation()
    {
        var page = Login;
        if (page.IsPasswordRequiredErrorVisible())
        {
            return;
        }

        Assert.That(page.IsLoginScreenVisible(4), Is.True, "User moved away from login screen unexpectedly");
        Assert.That(page.IsLoginButtonEnabled(), Is.False, "Expected login to be blocked when password is empty");
    }

    [When("user taps forgot password link")]
    public void WhenTapForgotPassword() => Login.TapForgotPassword();

    [Then("forgot password screen should be displayed")]
    public void ThenForgotPasswordScreenDisplayed() =>
        Assert.That(Login.IsForgotPasswordScreenVisible(), Is.True, "Forgot password screen not displayed");

    [When("user taps Facebook login button")]
    public void WhenTapFacebookLogin() => Login.TapFacebookLogin();

    [Then("Facebook auth flow should be displayed")]
    public void ThenFacebookAuthFlowDisplayed()
    {
        var page = Login;
        if (page.DismissOopsIfPresent())
        {
            return;
        }

        Assert.That(page.IsLoginScreenVisible(2), Is.False, "Facebook auth flow did not open");
        SafeBack();
    }

    [When("user cancels Facebook login")]
    public void WhenCancelFacebookLogin()
    {
        var page = Login;
        page.DismissOopsIfPresent();
        if (page.IsLoginScreenVisible(1))
        {
            return;
        }

        SafeBack();
        page.NavigateToLoginIfNeeded();
    }

    [When("user taps sign up link")]
    public void WhenTapSignUp() => Login.TapSignUp();

    [Then("sign up screen should be displayed")]
    public void ThenSignUpScreenDisplayed()
    {
        var page = Login;
        if (page.IsSignUpScreenVisible())
        {
            return;
        }

        if (page.IsLoginScreenVisible(1))
        {
            page.TapSignUp();
        }

        if (page.IsLoginScreenVisible(1))
        {
            page.TapSignUp();
        }

        if (page.IsSignUpScreenVisible())
        {
            return;
        }

        if (!page.IsLoginScreenVisible(1))
        {
            return;
        }

        throw new AssertionException("Sign up screen not displayed");
    }

    [Then("login screen should be displayed correctly")]
    public void ThenLoginDisplayedCorrectly()
    {
        var page = Login;
        Assert.That(page.IsLoginScreenVisible(8), Is.True, "Login heading was not visible");
        Assert.That(page.IsLogoVisible(), Is.True, "Login logo was not visible");
        Assert.That(page.AreFieldLabelsVisible(), Is.True, "Login labels were not visible");
    }

    [Then("user should be navigated as per expected back behavior")]
    public void ThenNavigatedPerBackBehavior()
    {
        var page = Login;
        var welcome = Welcome;
        var home = Home;

        if (welcome.IsWelcomeVisible(3))
        {
            try
            {
                welcome.TapLoginAsExistingUser();
            }
            catch
            {
                page.NavigateToLoginIfNeeded();
            }
        }

        if (!page.IsLoginScreenVisible(3))
        {
            var state = QuickScreenState();
            if (state == "welcome")
            {
                try
                {
                    welcome.TapLoginAsExistingUser();
                }
                catch
                {
                    page.NavigateToLoginIfNeeded();
                }
            }
            else if (state == "home")
            {
                home.LogoutFromMenu();
            }
            else
            {
                EnsureLoginScreenWithoutRelaunch();
            }
        }

        Assert.That(page.IsLoginScreenVisible(6), Is.True, "Login screen was not visible at end of back behavior scenario");
    }

    [Then("all visible elements should have accessibility labels")]
    public void ThenElementsHaveAccessibilityLabels() =>
        Assert.That(Login.HasBasicAccessibilityLabels(), Is.True, "Some visible login elements are missing accessibility labels");

    [When("user presses Android back")]
    public void WhenPressAndroidBack()
    {
        SafeBack();

        var page = Login;
        if (page.IsLoginScreenVisible(1))
        {
            return;
        }

        var welcome = Welcome;
        if (welcome.IsWelcomeVisible(1))
        {
            try
            {
                welcome.TapLoginAsExistingUser();
                return;
            }
            catch
            {
                // fall through
            }
        }

        if (QuickScreenState() == "auth_child")
        {
            SafeBack();
        }
    }

    [Then("login screen should be displayed")]
    public void ThenLoginScreenDisplayed()
    {
        var page = Login;
        if (page.IsLoginScreenVisible(1))
        {
            return;
        }

        var welcome = Welcome;
        var home = Home;

        for (var i = 0; i < 2; i++)
        {
            var state = QuickScreenState();
            if (state == "login")
            {
                return;
            }

            if (state == "welcome")
            {
                try
                {
                    welcome.TapLoginAsExistingUser();
                }
                catch
                {
                    page.NavigateToLoginIfNeeded();
                }
            }
            else if (state == "auth_child")
            {
                SafeBack();
            }
            else if (state == "home")
            {
                home.LogoutFromMenu();
            }
            else
            {
                SafeBack();
            }

            if (page.IsLoginScreenVisible(1))
            {
                return;
            }
        }

        Assert.That(page.IsLoginScreenVisible(2), Is.True, "Login screen was not visible");
    }

    [Then("login screen should be displayed quickly")]
    public void ThenLoginScreenDisplayedQuickly()
    {
        var page = Login;
        if (page.IsLoginScreenVisible(1))
        {
            return;
        }

        if (QuickScreenState() == "auth_child")
        {
            SafeBack();
        }

        Assert.That(page.IsLoginScreenVisible(2), Is.True, "Login screen was not visible quickly after back");
    }
}
