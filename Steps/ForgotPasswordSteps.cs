using System.Diagnostics;
using NUnit.Framework;
using OpenQA.Selenium;
using Reqnroll;
using Shoppix.MobileApp.Automation.Core;
using Shoppix.MobileApp.Automation.Pages;
using Shoppix.MobileApp.Automation.Support;
using static Shoppix.MobileApp.Automation.Support.StepHelpers;

namespace Shoppix.MobileApp.Automation.Steps;

/// <summary>Steps for the Forgot password screen feature.</summary>
[Binding]
[Scope(Feature = "Forgot password screen")]
public sealed class ForgotPasswordSteps
{
    private const string TempPasswordKey = "forgot_temp_password";
    private const string ResetRequestedAtKey = "forgot_reset_requested_at";
    private const string UnregisteredResetEmail = "unknown.user@shoppix.test";

    private readonly ScenarioContext _context;

    public ForgotPasswordSteps(ScenarioContext context) => _context = context;

    private ForgotPasswordPage Forgot => new(Driver);
    private LoginPage Login => new(Driver);
    private HomePage Home => new(Driver);

    private string ResetEmail => TestHost.GmailAddress.Length > 0 ? TestHost.GmailAddress : TestHost.Email;

    [Given("user is on forgot password screen")]
    public void GivenUserOnForgotPasswordScreen()
    {
        var forgot = Forgot;
        if (!forgot.IsForgotPasswordScreenVisible(1))
        {
            var login = Login;
            if (forgot.IsConfirmationAlertVisible(1))
            {
                forgot.TapGoToLogin();
            }

            EnsureLoginScreen(login);
            login.TapForgotPassword();
            Assert.That(forgot.IsForgotPasswordScreenVisible(8), Is.True, "Forgot password screen was not displayed");
        }

        forgot.ResetForm();
    }

    private void EnsureLoginScreen(LoginPage login)
    {
        if (login.IsLoginScreenVisible(2))
        {
            return;
        }

        for (var i = 0; i < 3; i++)
        {
            if (login.IsLoginScreenVisible(1))
            {
                return;
            }

            login.NavigateToLoginIfNeeded();
            if (login.IsLoginScreenVisible(2))
            {
                return;
            }

            SafeBack();
        }

        Assert.That(login.IsLoginScreenVisible(4), Is.True, "Could not return to login screen to open forgot password");
    }

    [Then("forgot password screen should display expected UI content")]
    public void ThenForgotDisplaysExpectedContent() =>
        Assert.That(Forgot.DisplaysExpectedUiContent(), Is.True, "Forgot password screen UI content not fully displayed");

    [Then("forgot password heading should be visible")]
    public void ThenForgotHeadingVisible() =>
        Assert.That(Forgot.IsHeadingVisible(), Is.True, "Forgot password heading not visible");

    [Then("forgot password description text should be visible")]
    public void ThenForgotDescriptionVisible() =>
        Assert.That(Forgot.IsDescriptionVisible(), Is.True, "Forgot password description text not visible");

    [Then("forgot password email field label should be visible")]
    public void ThenForgotEmailLabelVisible() =>
        Assert.That(Forgot.IsEmailLabelVisible(), Is.True, "Forgot password email field label not visible");

    [Then("forgot password email input should show placeholder text")]
    public void ThenForgotEmailPlaceholderVisible() =>
        Assert.That(Forgot.IsEmailPlaceholderVisible(), Is.True, "Forgot password email placeholder not visible");

    [Then("forgot password Send button should be visible")]
    public void ThenForgotSendButtonVisible() =>
        Assert.That(Forgot.IsSendButtonVisible(), Is.True, "Forgot password Send button not visible");

    [Then("forgot password back button should be visible")]
    public void ThenForgotBackButtonVisible() =>
        Assert.That(Forgot.IsBackButtonVisible(), Is.True, "Forgot password back button not visible");

    [When("user enters an invalid email for reset")]
    public void WhenEnterInvalidResetEmail() => Forgot.EnterEmail("invalid-email");

    [When("user leaves reset email field empty")]
    public void WhenLeaveResetEmailEmpty() => Forgot.EnterEmail("");

    [When("user enters an unregistered email for reset")]
    public void WhenEnterUnregisteredResetEmail() => Forgot.EnterEmail(UnregisteredResetEmail);

    [When("user enters a valid registered email for reset")]
    [When("user enters a registered email for reset")]
    public void WhenEnterRegisteredResetEmail()
    {
        Assert.That(string.IsNullOrWhiteSpace(ResetEmail), Is.False,
            "Missing GMAIL_ADDRESS (or SHOPPIX_LOGIN_EMAIL) in .env for the forgot password scenario");
        Forgot.EnterEmail(ResetEmail);
    }

    [Then("user cant tap forgot password Send button")]
    public void ThenCannotTapSendButton() =>
        Assert.That(Forgot.IsSendButtonEnabled(), Is.False, "Send button was enabled for an invalid/empty email");

    [Then("forgot password email input should show entered value")]
    public void ThenForgotEmailShowsEnteredValue()
    {
        var actual = Forgot.GetEmailValue();
        Assert.That(actual.Trim().ToLowerInvariant(), Is.EqualTo(ResetEmail.Trim().ToLowerInvariant()),
            "Entered reset email value not reflected");
    }

    [Then("Send button should get enabled")]
    public void ThenSendButtonEnabled() =>
        Assert.That(Forgot.WaitUntilSendEnabled(6), Is.True, "Send button did not become enabled for a valid email");

    [When("user taps forgot password Send button")]
    public void WhenTapSendButton()
    {
        _context[ResetRequestedAtKey] = DateTime.UtcNow;
        Assert.That(Forgot.TapSend(), Is.True, "Unable to tap the Send button");
    }

    [Then("reset link confirmation message should be displayed")]
    public void ThenResetConfirmationDisplayed() =>
        Assert.That(Forgot.IsConfirmationAlertVisible(), Is.True, "Reset confirmation message was not displayed");

    [Then("unregistered reset email behavior should be handled")]
    public void ThenUnregisteredResetHandled() =>
        Assert.That(Forgot.IsConfirmationAlertVisible(), Is.True,
            "Generic confirmation message was not displayed for an unregistered email");

    [When("user retrieves the temporary password from email")]
    public void WhenRetrieveTemporaryPassword()
    {
        Assert.That(string.IsNullOrWhiteSpace(TestHost.GmailAddress), Is.False,
            "Missing GMAIL_ADDRESS in .env for the temporary password scenario");
        Assert.That(string.IsNullOrWhiteSpace(TestHost.GmailAppPassword), Is.False,
            "Missing GMAIL_APP_PASSWORD in .env for the temporary password scenario");

        var requestedAt = _context.TryGetValue(ResetRequestedAtKey, out DateTime value)
            ? value
            : DateTime.UtcNow.AddMinutes(-2);

        var tempPassword = GmailReader.FetchTemporaryPassword(
            TestHost.GmailAddress,
            TestHost.GmailAppPassword,
            requestedAt);

        Assert.That(string.IsNullOrWhiteSpace(tempPassword), Is.False,
            "Temporary password email was not received or could not be parsed");
        _context[TempPasswordKey] = tempPassword!;
    }

    [When("user logs in with the temporary password")]
    public void WhenLoginWithTemporaryPassword()
    {
        Assert.That(_context.TryGetValue(TempPasswordKey, out string tempPassword) && tempPassword.Length > 0, Is.True,
            "Temporary password was not retrieved before login");

        var forgot = Forgot;
        if (forgot.IsConfirmationAlertVisible(1))
        {
            forgot.TapGoToLogin();
        }

        var login = Login;
        EnsureLoginScreen(login);
        login.ResetForm();
        login.LoginWithCredentials(ResetEmail, tempPassword);
    }

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

        Assert.That(home.IsHomeDashboardVisible(5), Is.True, "Home was not visible after login with temporary password");
    }

    [When("user taps forgot password back button")]
    public void WhenTapForgotBackButton() => Forgot.TapBack();

    [When("user presses Android back")]
    public void WhenPressAndroidBack() => SafeBack();

    [Then("login screen should be displayed")]
    public void ThenLoginScreenDisplayed()
    {
        var login = Login;
        if (login.IsLoginScreenVisible(2))
        {
            return;
        }

        EnsureLoginScreen(login);
        Assert.That(login.IsLoginScreenVisible(4), Is.True, "Login screen was not displayed");
    }
}
