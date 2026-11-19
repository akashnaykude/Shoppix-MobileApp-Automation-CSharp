using System.Diagnostics;
using NUnit.Framework;
using Reqnroll;
using Shoppix.MobileApp.Automation.Pages;
using Shoppix.MobileApp.Automation.Support;
using static Shoppix.MobileApp.Automation.Support.StepHelpers;

namespace Shoppix.MobileApp.Automation.Steps;

/// <summary>Port of Steps/Registration_steps.py (scoped to the Registration feature).</summary>
[Binding]
[Scope(Feature = "Registration screen")]
public sealed class RegistrationSteps
{
    private const string CreatePassword = "Test@123";
    private const string WeakPassword = "weak";
    private const string InvalidEmail = "invalid-email";

    private const string RegistrationSkippedMessage =
        "Registration.feature scenarios were skipped because Waiting list is enabled in manage shoppix for this " +
        "environment or check Threshold of Live Buyers in manage shoppix.";

    // Session-scoped environment classification: "waitlist" | "registration".
    private static string? _envMode;

    private readonly ScenarioContext _context;

    public RegistrationSteps(ScenarioContext context) => _context = context;

    private RegistrationPage Registration => new(Driver);

    private static string UniqueEmail() => $"autoreg_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}@test.com";

    private string ClassifyScreen()
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

        if (src.Contains("enter your postcode") || src.Contains("thank you for your interest"))
        {
            return "waitlist";
        }

        if (src.Contains("confirm password") || src.Contains("confirm your password")
            || src.Contains("create your password") || src.Contains("welcome to shoppix"))
        {
            return "registration";
        }

        return "unknown";
    }

    private string PollClassify(double timeout = 6.0)
    {
        var sw = Stopwatch.StartNew();
        var mode = "unknown";
        while (sw.Elapsed.TotalSeconds < timeout)
        {
            mode = ClassifyScreen();
            if (mode != "unknown")
            {
                return mode;
            }

            Thread.Sleep(400);
        }

        return mode;
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

        if (src.Contains("let's get started") || src.Contains("lets get started") || src.Contains("let\u2019s get started"))
        {
            return "welcome";
        }

        if (src.Contains("menu_action_setting") || src.Contains("img_header"))
        {
            return "home";
        }

        if (src.Contains("confirm password") || src.Contains("confirm your password")
            || src.Contains("create your password") || src.Contains("welcome to shoppix"))
        {
            return "registration";
        }

        if (src.Contains("terms and conditions") || src.Contains("privacy policy"))
        {
            return "legal";
        }

        return "unknown";
    }

    private RegistrationPage EnsureRegistrationScreen()
    {
        var page = Registration;

        if (_envMode == "waitlist")
        {
            Assert.Ignore(RegistrationSkippedMessage);
        }

        bool SettleOnRegistration()
        {
            page.RestoreRegistrationTop(4);
            return page.IsRegistrationScreenVisible(3);
        }

        for (var attempt = 0; attempt < 3; attempt++)
        {
            var mode = ClassifyScreen();

            if (mode == "waitlist")
            {
                _envMode = "waitlist";
                Assert.Ignore(RegistrationSkippedMessage);
            }

            if (mode == "registration" && SettleOnRegistration())
            {
                _envMode = "registration";
                return page;
            }

            var state = QuickScreenState();
            if (state == "legal")
            {
                SafeBack();
            }
            else if (state == "home")
            {
                new HomePage(Driver).LogoutFromMenu();
            }
            else if (state == "login")
            {
                try
                {
                    new LoginPage(Driver).TapSignUp();
                }
                catch
                {
                    SafeBack();
                    if (new WelcomePage(Driver).IsWelcomeVisible(2))
                    {
                        new WelcomePage(Driver).TapEmailRegistrationIcon();
                    }
                }
            }
            else if (state == "welcome")
            {
                new WelcomePage(Driver).TapEmailRegistrationIcon();
            }
            else if (state != "registration")
            {
                SafeBack();
                if (new WelcomePage(Driver).IsWelcomeVisible(2))
                {
                    new WelcomePage(Driver).TapEmailRegistrationIcon();
                }
            }

            var resolved = PollClassify(5);
            if (resolved == "waitlist")
            {
                _envMode = "waitlist";
                Assert.Ignore(RegistrationSkippedMessage);
            }

            if (resolved == "registration" && SettleOnRegistration())
            {
                _envMode = "registration";
                return page;
            }

            if (attempt == 2)
            {
                SoftRelaunchApp();
                if (new WelcomePage(Driver).IsWelcomeVisible(6))
                {
                    new WelcomePage(Driver).TapEmailRegistrationIcon();
                }

                if (PollClassify(6) == "waitlist")
                {
                    _envMode = "waitlist";
                    Assert.Ignore(RegistrationSkippedMessage);
                }
            }
        }

        if (SettleOnRegistration())
        {
            _envMode = "registration";
            return page;
        }

        Assert.That(page.IsRegistrationScreenVisible(6), Is.True, "Registration screen was not visible");
        _envMode = "registration";
        return page;
    }

    private bool ToggleUntil(RegistrationPage page, string field, bool expectedVisible, string password, int taps = 2)
    {
        for (var i = 0; i < taps; i++)
        {
            if (field == "create")
            {
                page.TapCreatePasswordToggle();
                if (expectedVisible && page.WaitUntilCreatePasswordVisibleAs(password, 1.5))
                {
                    return true;
                }

                if (!expectedVisible && page.WaitUntilCreatePasswordHiddenFrom(password, 1.5))
                {
                    return true;
                }
            }
            else
            {
                page.TapConfirmPasswordToggle();
                if (expectedVisible && page.WaitUntilConfirmPasswordVisibleAs(password, 1.5))
                {
                    return true;
                }

                if (!expectedVisible && page.WaitUntilConfirmPasswordHiddenFrom(password, 1.5))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void AssertRegistrationSuccessAndCleanup()
    {
        var home = new HomePage(Driver);
        var state = home.WaitForPostRegistrationState(40);

        if (state == "rate_limited")
        {
            home.DismissOopsIfPresent(2);
            Assert.Ignore("Registration throttled by backend: Too many requests. Please try again later.");
        }

        if (state == "tutorial")
        {
            Assert.That(home.SkipTutorialIfPresent(30), Is.True, "Tutorial skip did not reach home screen");
        }
        else if (state == "home")
        {
            // already home
        }
        else
        {
            Assert.Fail($"Registration did not reach tutorial/home. Observed state: {state}");
        }

        Assert.That(home.CleanupNewlyRegisteredAccount(CreatePassword), Is.True, "Registration succeeded but cleanup (delete account) failed");
    }

    private string CurrentCreatePassword() => _context.TryGetValue("current_create_password", out string value) ? value : CreatePassword;

    [Given("user is on registration screen")]
    public void GivenUserOnRegistrationScreen()
    {
        var page = EnsureRegistrationScreen();
        page.ResetForm();
    }

    [Then("registration screen should load successfully")]
    public void ThenRegistrationLoads() =>
        Assert.That(Registration.IsRegistrationScreenVisible(6), Is.True, "Registration screen did not load");

    [Then("app logo should be visible")]
    public void ThenLogoVisible() => Assert.That(Registration.IsLogoVisible(), Is.True, "App logo not visible on registration screen");

    [Then("welcome heading should be visible")]
    public void ThenHeadingVisible() => Assert.That(Registration.IsHeadingVisible(), Is.True, "Welcome heading not visible on registration screen");

    [Then("subtitle text should be visible")]
    public void ThenSubtitleVisible() => Assert.That(Registration.IsSubtitleVisible(), Is.True, "Subtitle not visible on registration screen");

    [Then("back button should be visible")]
    public void ThenBackButtonVisible() => Assert.That(Registration.IsBackButtonVisible(), Is.True, "Back button not visible on registration screen");

    [Then("email field label should be visible")]
    public void ThenEmailLabelVisible() => Assert.That(Registration.IsEmailLabelVisible(), Is.True, "Email field label not visible");

    [Then("email input placeholder should be visible")]
    public void ThenEmailPlaceholderVisible() => Assert.That(Registration.IsEmailPlaceholderVisible(), Is.True, "Email placeholder not visible");

    [Then("create password field label should be visible")]
    public void ThenCreatePasswordLabelVisible() => Assert.That(Registration.IsCreatePasswordLabelVisible(), Is.True, "Create password label not visible");

    [Then("create password input placeholder should be visible")]
    public void ThenCreatePasswordPlaceholderVisible() => Assert.That(Registration.IsCreatePasswordPlaceholderVisible(), Is.True, "Create password placeholder not visible");

    [Then("confirm password field label should be visible")]
    public void ThenConfirmPasswordLabelVisible() => Assert.That(Registration.IsConfirmPasswordLabelVisible(), Is.True, "Confirm password label not visible");

    [Then("confirm password input placeholder should be visible")]
    public void ThenConfirmPasswordPlaceholderVisible() => Assert.That(Registration.IsConfirmPasswordPlaceholderVisible(), Is.True, "Confirm password placeholder not visible");

    [Then("referral code label should be visible")]
    public void ThenReferralLabelVisible() => Assert.That(Registration.IsReferralLabelVisible(), Is.True, "Referral code label not visible");

    [Then("referral code input placeholder should be visible")]
    public void ThenReferralPlaceholderVisible() => Assert.That(Registration.IsReferralPlaceholderVisible(), Is.True, "Referral code placeholder not visible");

    [Then("age confirmation checkbox should be visible")]
    public void ThenAgeCheckboxVisible() => Assert.That(Registration.IsAgeCheckboxVisible(), Is.True, "Age confirmation checkbox not visible");

    [Then("terms and conditions text should be visible")]
    public void ThenTermsTextVisible() => Assert.That(Registration.IsTermsAndConditionsTextVisible(), Is.True, "Terms and Conditions text not visible");

    [Then("privacy policy text should be visible")]
    public void ThenPrivacyTextVisible() => Assert.That(Registration.IsPrivacyPolicyTextVisible(), Is.True, "Privacy Policy text not visible");

    [Then("continue button should be visible")]
    public void ThenContinueVisible() => Assert.That(Registration.IsContinueButtonVisible(), Is.True, "Continue button not visible");

    [Then("social registration section text should be visible")]
    public void ThenSocialRegTextVisible() => Assert.That(Registration.IsSocialRegistrationTextVisible(), Is.True, "Social registration section text not visible");

    [Then("Facebook registration button should be visible")]
    public void ThenFacebookRegButtonVisible() => Assert.That(Registration.IsFacebookRegistrationVisible(), Is.True, "Facebook registration button not visible");

    [When("user enters valid email address")]
    public void WhenEnterValidEmail()
    {
        var email = UniqueEmail();
        Registration.EnterEmail(email);
        _context["registration_email"] = email;
    }

    [Then("email input should show entered value")]
    public void ThenEmailShowsValue()
    {
        var page = Registration;
        var expected = _context.TryGetValue("registration_email", out string value) ? value : "";
        var actual = page.GetEmailValue().Trim().ToLowerInvariant();
        if (expected.Length > 0)
        {
            Assert.That(actual, Is.EqualTo(expected.Trim().ToLowerInvariant()), "Entered email value not reflected");
        }
        else
        {
            Assert.That(actual, Does.Contain("@"), "Entered email value was not retained");
        }
    }

    [When("user enters invalid email address")]
    public void WhenEnterInvalidEmail() => Registration.EnterEmail(InvalidEmail);

    [Then("email format validation message should be displayed")]
    public void ThenEmailFormatValidation() => Assert.That(Registration.IsEmailFormatErrorVisible(), Is.True, "Email format validation message not displayed");

    [When("user leaves email field empty")]
    public void WhenLeaveEmailEmpty() => Registration.EnterEmail("");

    [Then("email required validation message should be displayed")]
    public void ThenEmailRequiredValidation() => Assert.That(Registration.IsEmailRequiredErrorVisible(), Is.True, "Email required validation not displayed");

    [When("user enters create password")]
    public void WhenEnterCreatePassword()
    {
        Registration.EnterCreatePassword(CreatePassword);
        _context["current_create_password"] = CreatePassword;
    }

    [Then("create password input should show masked value")]
    public void ThenCreatePasswordMasked() => Assert.That(Registration.IsCreatePasswordMasked(), Is.True, "Create password was not masked");

    [Then("create password should be hidden by default")]
    public void ThenCreatePasswordHiddenByDefault() => Assert.That(Registration.IsCreatePasswordMasked(), Is.True, "Create password is not hidden by default");

    [When("user taps create password visibility toggle icon")]
    public void WhenTapCreatePasswordToggle()
    {
        var page = Registration;
        var togglePassword = _context.TryGetValue("create_toggle_password", out string value) ? value : CreatePassword;
        if (page.GetEmailValue().Length == 0)
        {
            page.EnterEmail(UniqueEmail());
        }

        page.EnterCreatePassword(togglePassword);
        _context["create_toggle_password"] = togglePassword;
        Assert.That(ToggleUntil(page, "create", true, togglePassword), Is.True, "Unable to toggle create password visibility ON");
    }

    [Then("create password should be visible")]
    public void ThenCreatePasswordVisible()
    {
        var pwd = _context.TryGetValue("create_toggle_password", out string value) ? value : CreatePassword;
        Assert.That(Registration.WaitUntilCreatePasswordVisibleAs(pwd, 2), Is.True, "Create password did not become visible");
    }

    [When("user taps create password visibility toggle icon again")]
    public void WhenTapCreatePasswordToggleAgain()
    {
        var page = Registration;
        var pwd = _context.TryGetValue("create_toggle_password", out string value) ? value : CreatePassword;
        Assert.That(ToggleUntil(page, "create", false, pwd), Is.True, "Unable to toggle create password visibility OFF");
    }

    [Then("create password should be hidden")]
    public void ThenCreatePasswordHidden()
    {
        var pwd = _context.TryGetValue("create_toggle_password", out string value) ? value : CreatePassword;
        Assert.That(Registration.WaitUntilCreatePasswordHiddenFrom(pwd, 2), Is.True, "Create password did not become hidden");
    }

    [When("user enters weak create password")]
    public void WhenEnterWeakCreatePassword()
    {
        Registration.EnterCreatePassword(WeakPassword);
        _context["current_create_password"] = WeakPassword;
    }

    [Then("password strength validation message should be displayed")]
    public void ThenPasswordStrengthValidation() => Assert.That(Registration.IsPasswordStrengthErrorVisible(), Is.True, "Password strength validation was not displayed");

    [When("user leaves create password field empty")]
    public void WhenLeaveCreatePasswordEmpty()
    {
        Registration.EnterCreatePassword("");
        _context["current_create_password"] = "";
    }

    [Then("create password required validation message should be displayed")]
    public void ThenCreatePasswordRequiredValidation() => Assert.That(Registration.IsCreatePasswordRequiredErrorVisible(), Is.True, "Create password required validation not displayed");

    [When("user enters confirm password")]
    public void WhenEnterConfirmPassword() => Registration.EnterConfirmPassword(CurrentCreatePassword());

    [Then("confirm password input should show masked value")]
    public void ThenConfirmPasswordMasked() => Assert.That(Registration.IsConfirmPasswordMasked(), Is.True, "Confirm password was not masked");

    [Then("confirm password should be hidden by default")]
    public void ThenConfirmPasswordHiddenByDefault() => Assert.That(Registration.IsConfirmPasswordMasked(), Is.True, "Confirm password is not hidden by default");

    [When("user taps confirm password visibility toggle icon")]
    public void WhenTapConfirmPasswordToggle()
    {
        var page = Registration;
        var togglePassword = _context.TryGetValue("confirm_toggle_password", out string value) ? value : CreatePassword;
        page.EnterConfirmPassword(togglePassword);
        _context["confirm_toggle_password"] = togglePassword;
        Assert.That(ToggleUntil(page, "confirm", true, togglePassword), Is.True, "Unable to toggle confirm password visibility ON");
    }

    [Then("confirm password should be visible")]
    public void ThenConfirmPasswordVisible()
    {
        var pwd = _context.TryGetValue("confirm_toggle_password", out string value) ? value : CreatePassword;
        Assert.That(Registration.WaitUntilConfirmPasswordVisibleAs(pwd, 2), Is.True, "Confirm password did not become visible");
    }

    [When("user taps confirm password visibility toggle icon again")]
    public void WhenTapConfirmPasswordToggleAgain()
    {
        var page = Registration;
        var pwd = _context.TryGetValue("confirm_toggle_password", out string value) ? value : CreatePassword;
        Assert.That(ToggleUntil(page, "confirm", false, pwd), Is.True, "Unable to toggle confirm password visibility OFF");
    }

    [Then("confirm password should be hidden")]
    public void ThenConfirmPasswordHidden()
    {
        var pwd = _context.TryGetValue("confirm_toggle_password", out string value) ? value : CreatePassword;
        Assert.That(Registration.WaitUntilConfirmPasswordHiddenFrom(pwd, 2), Is.True, "Confirm password did not become hidden");
    }

    [When("user enters matching confirm password")]
    public void WhenEnterMatchingConfirmPassword() => Registration.EnterConfirmPassword(CurrentCreatePassword());

    [Then("passwords should match successfully")]
    public void ThenPasswordsMatch() => Assert.That(Registration.IsPasswordMismatchErrorVisible(), Is.False, "Password mismatch error displayed for matching passwords");

    [When("user enters non matching confirm password")]
    public void WhenEnterNonMatchingConfirmPassword() => Registration.EnterConfirmPassword("Mismatch@123");

    [Then("password mismatch validation message should be displayed")]
    public void ThenPasswordMismatchValidation() => Assert.That(Registration.IsPasswordMismatchErrorVisible(), Is.True, "Password mismatch validation message not displayed");

    [When("user leaves confirm password field empty")]
    public void WhenLeaveConfirmPasswordEmpty() => Registration.EnterConfirmPassword("");

    [Then("confirm password required validation message should be displayed")]
    public void ThenConfirmPasswordRequiredValidation() => Assert.That(Registration.IsConfirmPasswordRequiredErrorVisible(), Is.True, "Confirm password required validation not displayed");

    [When("user enters referral code")]
    public void WhenEnterReferralCode() => Registration.EnterReferralCode("ABC123");

    [Then("referral code input should show entered value")]
    public void ThenReferralShowsValue() => Assert.That(Registration.GetReferralValue(), Is.Not.EqualTo(""), "Referral code value was not retained");

    [When("user taps age confirmation checkbox")]
    public void WhenTapAgeCheckbox()
    {
        var page = Registration;
        for (var i = 0; i < 3; i++)
        {
            page.TapAgeCheckbox();
            if (page.WaitUntilAgeCheckboxState(true, 0.6))
            {
                return;
            }

            page.TapAgeCheckboxByRelativeLayout();
            if (page.WaitUntilAgeCheckboxState(true, 0.6))
            {
                return;
            }
        }
    }

    [Then("age confirmation checkbox should be selected")]
    public void ThenAgeCheckboxSelected() => Assert.That(Registration.WaitUntilAgeCheckboxState(true, 1.2), Is.True, "Age checkbox was not selected");

    [When("user taps age confirmation checkbox again")]
    public void WhenTapAgeCheckboxAgain() => Registration.SetAgeCheckboxState(false);

    [Then("age confirmation checkbox should be deselected")]
    public void ThenAgeCheckboxDeselected()
    {
        var page = Registration;
        page.TapContinue();
        Assert.That(page.IsConsentRequiredErrorVisible(), Is.True, "Age checkbox deselection was not applied");
    }

    [When("user taps terms and conditions link")]
    public void WhenTapTermsLink()
    {
        var page = Registration;
        page.ScrollToTermsAndConditionsLink();
        Assert.That(page.TapTermsAndConditionsTextSimple(), Is.True, "Could not open Terms & Conditions screen from the link");
    }

    [Then("terms and conditions screen should be displayed")]
    public void ThenTermsScreenDisplayed()
    {
        Assert.That(Registration.IsTermsScreenVisible(6), Is.True, "Terms and Conditions screen was not displayed");
        SafeBack();
    }

    [When("user taps privacy policy link")]
    public void WhenTapPrivacyLink()
    {
        var page = Registration;
        page.ScrollToPrivacyPolicyLink();
        Assert.That(page.TapPrivacyPolicyTextSimple(), Is.True, "Could not open Privacy Policy screen from the link");
    }

    [Then("privacy policy screen should be displayed")]
    public void ThenPrivacyScreenDisplayed()
    {
        Assert.That(Registration.IsPrivacyScreenVisible(6), Is.True, "Privacy Policy screen was not displayed");
        SafeBack();
    }

    [When("user enters valid create password")]
    public void WhenEnterValidCreatePassword()
    {
        Registration.EnterCreatePassword(CreatePassword);
        _context["current_create_password"] = CreatePassword;
    }

    [When("user taps continue button")]
    public void WhenTapContinue() => Registration.TapContinue();

    [Then("user should proceed to next registration step")]
    public void ThenProceedToNextStep()
    {
        var page = Registration;
        if (page.IsAlreadyRegisteredAlertVisible())
        {
            throw new AssertionException("Already registered alert displayed for unique registration flow");
        }

        AssertRegistrationSuccessAndCleanup();
    }

    [When("user enters already registered email address")]
    public void WhenEnterAlreadyRegisteredEmail()
    {
        var existingEmail = TestHost.Email;
        Assert.That(string.IsNullOrEmpty(existingEmail), Is.False, "Missing SHOPPIX_LOGIN_EMAIL in .env for already registered scenario");
        Registration.EnterEmail(existingEmail);
    }

    [Then("already registered alert should be displayed")]
    public void ThenAlreadyRegisteredAlert() => Assert.That(Registration.IsAlreadyRegisteredAlertVisible(), Is.True, "Already registered alert was not displayed");

    [When("user taps already registered alert OK button")]
    public void WhenTapAlreadyRegisteredOk() => Registration.DismissAlreadyRegisteredAlert();

    [When("user taps Facebook registration button")]
    public void WhenTapFacebookRegistration() => Registration.TapFacebookRegistration();

    [Then("Facebook auth flow should be displayed")]
    public void ThenFacebookAuthFlowDisplayed()
    {
        var page = Registration;
        Assert.That(page.IsFacebookAuthFlowDisplayed(), Is.True, "Facebook auth flow did not open");

        if (!page.IsRegistrationScreenVisible(2))
        {
            SafeBack();
        }
    }

    [When("user cancels Facebook registration")]
    public void WhenCancelFacebookRegistration() => SafeBack();

    [Then("registration screen should be displayed")]
    public void ThenRegistrationScreenDisplayed() =>
        Assert.That(EnsureRegistrationScreen().IsRegistrationScreenVisible(6), Is.True, "Registration screen was not displayed");

    [When("app is sent to background")]
    public void WhenAppSentToBackground() => BackgroundApp(2);

    [When("app is brought to foreground")]
    public void WhenAppBroughtToForeground()
    {
        Assert.That(string.IsNullOrEmpty(AppPackage), Is.False, "App package not available in capabilities");
        ActivateApp();
    }

    [When("device orientation is changed")]
    public void WhenOrientationChanged() => ChangeOrientation();

    [Then("registration screen should be displayed correctly")]
    public void ThenRegistrationDisplayedCorrectly()
    {
        var page = EnsureRegistrationScreen();
        Assert.That(page.IsCoreContentVisible(), Is.True, "Registration screen did not render correctly");
    }

    [When("user presses Android back")]
    public void WhenPressAndroidBack() => SafeBack();

    [Then("user should be navigated as per expected back behavior")]
    public void ThenNavigatedPerBackBehavior()
    {
        var state = QuickScreenState();
        Assert.That(new[] { "welcome", "login", "registration", "home" }, Does.Contain(state), "Back navigation did not land on a known app screen");
    }

    [Then("all visible elements should have accessibility labels")]
    public void ThenElementsHaveAccessibilityLabels()
    {
        var page = Registration;
        Assert.That(page.HasBasicAccessibilityLabels(), Is.True,
            "One or more key registration elements are missing accessibility labels: " + string.Join(", ", page.LastAccessibilityMissing()));
        Assert.That(page.RestoreRegistrationTop(6), Is.True, "Could not restore registration screen to top (Welcome to Shoppix heading not visible)");
    }

    [Then("registration screen should render correctly on small screen device")]
    public void ThenRendersOnSmallScreen() =>
        Assert.That(EnsureRegistrationScreen().IsCoreContentVisible(), Is.True, "Registration layout integrity failed for small-screen check");

    [Then("registration screen should render correctly on large screen device")]
    public void ThenRendersOnLargeScreen() =>
        Assert.That(EnsureRegistrationScreen().IsCoreContentVisible(), Is.True, "Registration layout integrity failed for large-screen check");
}
