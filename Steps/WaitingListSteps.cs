using System.Diagnostics;
using NUnit.Framework;
using Reqnroll;
using Shoppix.MobileApp.Automation.Pages;
using Shoppix.MobileApp.Automation.Support;
using static Shoppix.MobileApp.Automation.Support.StepHelpers;

namespace Shoppix.MobileApp.Automation.Steps;

/// <summary>Port of Steps/RegistrationFlowWithWaitingListEnabled_steps.py.</summary>
[Binding]
[Scope(Feature = "Registration flow with waiting list enabled")]
public sealed class WaitingListSteps
{
    private const string ValidUkPostcode = "SW1A 1AA";
    private const string NonUkPostcode = "12345";
    private const string InvalidUkPostcode = "QW";
    private const string InvalidEmail = "a@hsbsbsjsn";
    private const string ReferralCode = "FRIEND50";
    private const string DefaultWaitlistDuplicateEmail = "test5@test.com";

    private static readonly bool WaitlistDuplicateSelfHeal =
        (Environment.GetEnvironmentVariable("WAITLIST_DUPLICATE_SELF_HEAL") ?? "0").Trim() == "1";

    private const string WaitlistDisabledSkipMessage =
        "Waiting list is disabled in this environment, so RegistrationFlowWithWaitingListEnabled.feature scenarios were " +
        "skipped. Update Threshold of Live Buyers in manage shoppix to enable waiting list.";

    private static string? _envMode;
    private static string? _seededWaitlistEmail;

    private readonly ScenarioContext _context;

    public WaitingListSteps(ScenarioContext context) => _context = context;

    private WaitingListPage WaitingList => new(Driver);

    private static string UniqueEmail() => $"autowl_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}@test.com";

    private static string ConfiguredDuplicateEmail() =>
        (Environment.GetEnvironmentVariable("WAITLIST_DUPLICATE_EMAIL") ?? DefaultWaitlistDuplicateEmail).Trim().ToLowerInvariant();

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

        if (src.Contains("confirm password") || src.Contains("welcome to shoppix"))
        {
            return "registration";
        }

        return "unknown";
    }

    private WaitingListPage EnsureWaitingListScreen()
    {
        var page = WaitingList;

        if (_envMode == "registration")
        {
            Assert.Ignore(WaitlistDisabledSkipMessage);
        }

        if (page.IsWaitingListVisible(2))
        {
            _envMode = "waitlist";
            return page;
        }

        var welcome = new WelcomePage(Driver);
        if (!welcome.IsWelcomeVisible(2))
        {
            SafeBack();
        }

        if (welcome.IsWelcomeVisible(3))
        {
            welcome.TapEmailRegistrationIcon();
        }

        if (page.IsWaitingListVisible(4))
        {
            _envMode = "waitlist";
            return page;
        }

        if (ClassifyScreen() == "registration")
        {
            _envMode = "registration";
            Assert.Ignore(WaitlistDisabledSkipMessage);
        }

        Assert.Ignore("Could not reach the waiting list screen (entry point unavailable).");
        return page;
    }

    private WaitingListPage SubmitWaitlistForm(string email, bool doubleTap = false)
    {
        var page = WaitingList;
        if (!page.IsWaitingListVisible(2))
        {
            page = EnsureWaitingListScreen();
        }

        page.FillValidForm(email, ValidUkPostcode);
        if (doubleTap)
        {
            page.TapContinueTwice();
        }
        else
        {
            page.TapContinue();
        }

        return page;
    }

    private (string Email, bool DuplicateAlreadyVisible) ResolveDuplicateEmail()
    {
        if (!string.IsNullOrEmpty(_seededWaitlistEmail))
        {
            return (_seededWaitlistEmail!, false);
        }

        var email = ConfiguredDuplicateEmail();
        var page = SubmitWaitlistForm(email);
        if (page.IsDuplicateAlertVisible(4))
        {
            _seededWaitlistEmail = email;
            return (email, true);
        }

        if (!WaitlistDuplicateSelfHeal)
        {
            Assert.Fail(
                "Configured duplicate email did not trigger duplicate alert. " +
                "Use a known waitlisted email or set WAITLIST_DUPLICATE_SELF_HEAL=1 for one-time reseeding.");
        }

        if (page.IsSuccessAlertVisible(6))
        {
            page.TapDone();
            new WelcomePage(Driver).IsWelcomeVisible(10);
            page = SubmitWaitlistForm(email);
            if (page.IsDuplicateAlertVisible(4))
            {
                _seededWaitlistEmail = email;
                return (email, true);
            }
        }

        var fallback = UniqueEmail();
        page = SubmitWaitlistForm(fallback);
        Assert.That(page.IsSuccessAlertVisible(18), Is.True, "Could not seed waitlist email for duplicate scenario");
        page.TapDone();
        new WelcomePage(Driver).IsWelcomeVisible(10);
        page = SubmitWaitlistForm(fallback);
        Assert.That(page.IsDuplicateAlertVisible(4), Is.True, "Duplicate alert not shown after fallback seeding");
        _seededWaitlistEmail = fallback;
        return (fallback, true);
    }

    [Given("user is on waiting list registration screen")]
    public void GivenUserOnWaitingListScreen()
    {
        var page = EnsureWaitingListScreen();
        page.ResetForm();
        page.SetConsent(false);
    }

    [Given("user freshly reopens waiting list registration screen")]
    public void GivenUserFreshlyReopens()
    {
        var page = WaitingList;
        var welcome = new WelcomePage(Driver);
        if (page.IsWaitingListVisible(2))
        {
            SafeBack();
        }

        if (welcome.IsWelcomeVisible(3))
        {
            welcome.TapEmailRegistrationIcon();
        }

        page = EnsureWaitingListScreen();
        page.ResetForm();
        page.SetConsent(false);
    }

    [Then("waiting list registration screen should load successfully")]
    public void ThenScreenLoads() => Assert.That(WaitingList.IsWaitingListVisible(6), Is.True, "Waiting list screen did not load");

    [Then("app logo should be visible")]
    public void ThenLogoVisible() => Assert.That(WaitingList.IsLogoVisible(), Is.True, "App logo not visible");

    [Then("back button should be visible")]
    public void ThenBackVisible() => Assert.That(WaitingList.IsBackButtonVisible(), Is.True, "Back button not visible");

    [Then("main heading should be visible")]
    public void ThenHeadingVisible() => Assert.That(WaitingList.IsHeadingVisible(), Is.True, "Main heading not visible");

    [Then("waiting list description text should be visible")]
    public void ThenDescriptionVisible() => Assert.That(WaitingList.IsDescriptionTextVisible(), Is.True, "Description text not visible");

    [Then("referral code instruction text should be visible")]
    public void ThenReferralInstructionVisible() => Assert.That(WaitingList.IsReferralInstructionVisible(), Is.True, "Referral instruction text not visible");

    [Then("email field label should be visible")]
    public void ThenEmailLabelVisible() => Assert.That(WaitingList.IsEmailLabelVisible(), Is.True, "Email label not visible");

    [Then("email input placeholder should be visible")]
    public void ThenEmailPlaceholderVisible() => Assert.That(WaitingList.IsEmailPlaceholderVisible(), Is.True, "Email placeholder not visible");

    [Then("postcode field label should be visible")]
    public void ThenPostcodeLabelVisible() => Assert.That(WaitingList.IsPostcodeLabelVisible(), Is.True, "Postcode label not visible");

    [Then("postcode input placeholder should be visible")]
    public void ThenPostcodePlaceholderVisible() => Assert.That(WaitingList.IsPostcodePlaceholderVisible(), Is.True, "Postcode placeholder not visible");

    [Then("age range field label should be visible")]
    public void ThenAgeLabelVisible() => Assert.That(WaitingList.IsAgeLabelVisible(), Is.True, "Age range label not visible");

    [Then("age range dropdown should be visible")]
    public void ThenAgeDropdownVisible() => Assert.That(WaitingList.IsAgeDropdownVisible(), Is.True, "Age range dropdown not visible");

    [Then("referral code label should be visible")]
    public void ThenReferralLabelVisible() => Assert.That(WaitingList.IsReferralLabelVisible(), Is.True, "Referral code label not visible");

    [Then("referral code input placeholder should be visible")]
    public void ThenReferralPlaceholderVisible() => Assert.That(WaitingList.IsReferralPlaceholderVisible(), Is.True, "Referral code placeholder not visible");

    [Then("consent checkbox should be visible")]
    public void ThenConsentCheckboxVisible() => Assert.That(WaitingList.IsConsentCheckboxVisible(), Is.True, "Consent checkbox not visible");

    [Then("consent text should be visible")]
    public void ThenConsentTextVisible() => Assert.That(WaitingList.IsConsentTextVisible(), Is.True, "Consent text not visible");

    [Then("privacy information text should be visible")]
    public void ThenPrivacyNoteVisible() => Assert.That(WaitingList.IsPrivacyNoteVisible(), Is.True, "Privacy information text not visible");

    [Then("continue button should be visible")]
    public void ThenContinueVisible()
    {
        try
        {
            Assert.That(WaitingList.IsContinueButtonVisible(), Is.True, "Continue button not visible");
        }
        finally
        {
            SafeBack();
        }
    }

    [When("user enters valid email address")]
    public void WhenEnterValidEmail()
    {
        var email = UniqueEmail();
        _context["email"] = email;
        WaitingList.EnterEmail(email);
    }

    [Then("email input should show entered value")]
    public void ThenEmailShowsValue() =>
        Assert.That(WaitingList.GetEmailValue(), Is.EqualTo(_context.Get<string>("email")), "Email value mismatch");

    [When("user leaves email field empty and tries to continue")]
    public void WhenEmptyEmailContinue()
    {
        var page = WaitingList;
        page.EnterEmail("");
        page.TapContinue();
    }

    [Then("email required validation message should be displayed")]
    public void ThenEmailRequiredError() => Assert.That(WaitingList.IsEmailRequiredErrorVisible(), Is.True, "Email required error not shown");

    [When("user enters invalid email address")]
    public void WhenEnterInvalidEmail() => WaitingList.EnterEmail(InvalidEmail);

    [Then("email format validation message should be displayed")]
    public void ThenEmailInvalidError() => Assert.That(WaitingList.IsEmailInvalidErrorVisible(), Is.True, "Email invalid format error not shown");

    [When("user enters email address with leading and trailing spaces")]
    public void WhenEnterEmailWithSpaces()
    {
        _context["trim_email"] = "spaced.user@test.com";
        WaitingList.EnterEmail($"  {_context.Get<string>("trim_email")}  ");
    }

    [Then("email input should trim spaces correctly")]
    public void ThenEmailTrimmed() =>
        Assert.That(WaitingList.GetEmailValue(), Is.EqualTo(_context.Get<string>("trim_email")), "Email was not trimmed");

    [When("user enters postcode")]
    public void WhenEnterPostcode()
    {
        _context["postcode"] = ValidUkPostcode;
        WaitingList.EnterPostcode(ValidUkPostcode);
    }

    [Then("postcode input should show entered value")]
    public void ThenPostcodeShowsValue() =>
        Assert.That(WaitingList.GetPostcodeValue(), Is.EqualTo(_context.Get<string>("postcode")), "Postcode value mismatch");

    [When("user leaves postcode field empty and tries to continue")]
    public void WhenEmptyPostcodeContinue()
    {
        var page = WaitingList;
        page.EnterEmail(UniqueEmail());
        page.EnterPostcode("");
        page.TapContinue();
    }

    [Then("postcode required validation message should be displayed")]
    public void ThenPostcodeRequiredError() => Assert.That(WaitingList.IsPostcodeRequiredErrorVisible(), Is.True, "Postcode required error not shown");

    [When("user enters valid UK postcode")]
    public void WhenEnterValidUkPostcode() => WaitingList.EnterPostcode(ValidUkPostcode);

    [Then("postcode should be accepted without validation error")]
    public void ThenPostcodeAccepted() => Assert.That(WaitingList.IsPostcodeInvalidErrorPresent(), Is.False, "Valid UK postcode was flagged as invalid");

    [When("user enters non UK postcode")]
    public void WhenEnterNonUkPostcode() => WaitingList.EnterPostcode(NonUkPostcode);

    [When("user enters invalid UK postcode")]
    public void WhenEnterInvalidUkPostcode() => WaitingList.EnterPostcode(InvalidUkPostcode);

    [Then("valid postcode validation message should be displayed")]
    public void ThenPostcodeInvalidError() => Assert.That(WaitingList.IsPostcodeInvalidErrorVisible(), Is.True, "Invalid postcode error not shown");

    [When("user enters postcode with leading and trailing spaces")]
    public void WhenEnterPostcodeWithSpaces()
    {
        _context["trim_postcode"] = ValidUkPostcode;
        WaitingList.EnterPostcode($"  {ValidUkPostcode}  ");
    }

    [Then("postcode input should trim spaces correctly")]
    public void ThenPostcodeTrimmed() =>
        Assert.That(WaitingList.GetPostcodeValue(), Is.EqualTo(_context.Get<string>("trim_postcode")), "Postcode was not trimmed");

    [When("user taps age range dropdown")]
    public void WhenTapAgeDropdown() => WaitingList.OpenAgeDropdown();

    [Then("age range options should be displayed")]
    public void ThenAgeOptionsDisplayed() => Assert.That(WaitingList.IsAgeOptionsDisplayed(), Is.True, "Age range options not displayed");

    [Then("all age range options should be visible")]
    public void ThenAllAgeOptionsVisible() => Assert.That(WaitingList.AreAllAgeOptionsVisible(), Is.True, "Not all age range options are visible");

    [When("user selects an age range option")]
    public void WhenSelectAnAge()
    {
        _context["age"] = "25-34";
        WaitingList.SelectAgeRange("25-34");
    }

    [When("user selects {string} age range")]
    public void WhenSelectSpecificAge(string age)
    {
        _context["age"] = age;
        WaitingList.SelectAgeRange(age);
    }

    [Then("selected age range should be {string}")]
    public void ThenSelectedAgeEquals(string age) =>
        Assert.That(WaitingList.GetSelectedAge(), Is.EqualTo(age), $"Selected age is not {age}");

    [When("user leaves age range unselected and tries to continue")]
    public void WhenAgeUnselectedContinue()
    {
        var page = WaitingList;

        if (page.GetSelectedAge().Length > 0)
        {
            SafeBack();
            page = EnsureWaitingListScreen();
            page.ResetForm();
            page.SetConsent(false);
        }

        page.EnterEmail(UniqueEmail());
        page.EnterPostcode(ValidUkPostcode);
        page.SetConsent(true);
        page.TapContinue();
    }

    [Then("age range required validation message should be displayed")]
    public void ThenAgeRequiredError() => Assert.That(WaitingList.IsAgeRequiredErrorVisible(), Is.True, "Age required error not shown");

    [When("user enters referral code")]
    public void WhenEnterReferral()
    {
        _context["referral"] = ReferralCode;
        WaitingList.EnterReferralCode(ReferralCode);
    }

    [Then("referral code input should show entered value")]
    public void ThenReferralShowsValue() =>
        Assert.That(WaitingList.GetReferralValue(), Is.EqualTo(_context.Get<string>("referral")), "Referral value mismatch");

    [When("user leaves referral code empty")]
    public void WhenLeaveReferralEmpty() => WaitingList.EnterReferralCode("");

    [Then("referral code field should remain optional")]
    public void ThenReferralOptional()
    {
        var page = WaitingList;
        Assert.That(page.GetReferralValue(), Is.EqualTo(""), "Referral code should be empty");
        Assert.That(page.IsWaitingListVisible(3), Is.True, "Screen should stay usable with empty referral");
    }

    [When("user taps consent checkbox")]
    public void WhenTapConsent() => WaitingList.TapConsentCheckbox();

    [When("user taps consent checkbox again")]
    public void WhenTapConsentAgain() => WaitingList.TapConsentCheckbox();

    [Then("consent checkbox should be selected")]
    public void ThenConsentSelected() => Assert.That(WaitingList.IsConsentChecked(), Is.True, "Consent checkbox not selected");

    [Then("consent checkbox should be deselected")]
    public void ThenConsentDeselected() => Assert.That(WaitingList.IsConsentChecked(), Is.False, "Consent checkbox not deselected");

    [When("user taps continue button")]
    public void WhenTapContinue() => WaitingList.TapContinue();

    [When("user tries to continue")]
    public void WhenTryContinue() => WaitingList.TapContinue();

    [Then("required field validation messages should be displayed")]
    public void ThenRequiredValidations()
    {
        var page = WaitingList;
        var sw = Stopwatch.StartNew();
        var email = false;
        var postcode = false;
        var age = false;
        var consent = false;
        while (sw.Elapsed.TotalSeconds < 8)
        {
            email = page.IsEmailRequiredErrorVisible(2);
            postcode = page.IsPostcodeRequiredErrorVisible(2);
            age = page.IsAgeRequiredErrorVisible(2);
            consent = page.IsConsentRequiredErrorVisible(2);
            if (email && postcode && age && consent)
            {
                break;
            }

            Thread.Sleep(300);
        }

        Assert.That(email, Is.True, "Email required error not shown");
        Assert.That(postcode, Is.True, "Postcode required error not shown");
        Assert.That(age, Is.True, "Age required error not shown");
        Assert.That(consent, Is.True, "Consent required error not shown");
    }

    [Then("consent validation message should be displayed")]
    public void ThenConsentValidation() => Assert.That(WaitingList.IsConsentRequiredErrorVisible(), Is.True, "Consent required error not shown");

    [Then("success alert should be displayed")]
    public void ThenSuccessAlert() => Assert.That(WaitingList.IsSuccessAlertVisible(25), Is.True, "Success alert not displayed");

    [Then("success alert message should be shown")]
    public void ThenSuccessMessage() => Assert.That(WaitingList.IsSuccessAlertVisible(6), Is.True, "Success alert message not shown");

    [Then("Done button should be visible on success alert")]
    public void ThenDoneVisible() => Assert.That(WaitingList.IsDoneButtonVisible(), Is.True, "Done button not visible");

    [When("user taps Done button")]
    public void WhenTapDone() => WaitingList.TapDone();

    [Then("user should be redirected to welcome screen")]
    public void ThenRedirectedToWelcome() =>
        Assert.That(new WelcomePage(Driver).IsWelcomeVisible(12), Is.True, "Not redirected to welcome screen");

    [When("user submits a valid form using an email already on the waitlist")]
    public void WhenSubmitDuplicateEmail()
    {
        var (email, duplicateAlreadyVisible) = ResolveDuplicateEmail();
        if (!duplicateAlreadyVisible)
        {
            SubmitWaitlistForm(email);
        }
    }

    [When("user submits a valid form using an email already on the waitlist tapping continue twice quickly")]
    public void WhenSubmitDuplicateEmailDouble()
    {
        var (email, _) = ResolveDuplicateEmail();
        SubmitWaitlistForm(email, doubleTap: true);
    }

    [Then("duplicate waitlist alert should be displayed")]
    public void ThenDuplicateAlert() => Assert.That(WaitingList.IsDuplicateAlertVisible(), Is.True, "Duplicate waitlist alert not displayed");

    [Then("alert message should be {string}")]
    public void ThenAlertMessageEquals(string message) =>
        Assert.That(WaitingList.GetDuplicateAlertText(), Is.EqualTo(message), "Duplicate alert message mismatch");

    [Then("OK button should be visible on alert")]
    public void ThenOkVisible() => Assert.That(WaitingList.IsOkButtonVisible(), Is.True, "OK button not visible on alert");

    [When("user taps OK button")]
    public void WhenTapOk() => WaitingList.TapOk();

    [Then("alert should be dismissed")]
    public void ThenAlertDismissed() => Assert.That(WaitingList.IsAlertDismissed(), Is.True, "Alert was not dismissed");

    [Then("user should remain on waiting list registration screen")]
    public void ThenRemainOnScreen() => Assert.That(WaitingList.IsWaitingListVisible(6), Is.True, "User did not remain on waiting list screen");

    [Then("only one duplicate waitlist alert should be displayed")]
    public void ThenSingleDuplicateAlert()
    {
        var page = WaitingList;
        Assert.That(page.IsDuplicateAlertVisible(), Is.True, "Duplicate waitlist alert not displayed");
        Assert.That(page.DuplicateAlertCount(), Is.EqualTo(1), "More than one duplicate alert was shown");
    }

    [When("app is sent to background")]
    public void WhenAppSentToBackground()
    {
        try
        {
            Driver.BackgroundApp(TimeSpan.FromSeconds(2));
        }
        catch
        {
            SoftRelaunchApp();
        }
    }

    [When("app is brought to foreground")]
    public void WhenAppBroughtToForeground() => ActivateApp();

    [Then("waiting list registration screen should be displayed")]
    public void ThenScreenStillDisplayed() =>
        Assert.That(WaitingList.IsWaitingListVisible(10), Is.True, "Waiting list screen not displayed after resume");

    [When("device orientation is changed")]
    public void WhenOrientationChanged() => ChangeOrientation();

    [Then("waiting list registration screen should be displayed correctly")]
    public void ThenScreenDisplayedAfterRotation() =>
        Assert.That(WaitingList.IsWaitingListVisible(10), Is.True, "Waiting list screen not stable after rotation");

    [Then("key elements should have accessibility labels")]
    public void ThenAccessibilityLabels()
    {
        var status = WaitingList.KeyElementsAccessibilityStatus();
        Assert.That(status.Values.All(v => v), Is.True,
            $"Key elements missing accessibility labels: {string.Join(", ", status.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
    }

    [Then("waiting list registration screen should render core content correctly")]
    public void ThenCoreRender() => Assert.That(WaitingList.CoreContentRendered(), Is.True, "Core content did not render correctly");
}
