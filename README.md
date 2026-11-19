# Shoppix MobileApp Automation (C#)

Android UI automation using C#, Appium, Reqnroll (BDD) and NUnit. This is the
C# port of the original Python (pytest + pytest-bdd) suite.

## Stack
- .NET (targets `net8.0`) + Appium.WebDriver 5.x
- Reqnroll.NUnit (Gherkin / BDD) + NUnit 3
- POM-style page objects
- JSON-based run profiles and test data
- Allure reporting (`Allure.Reqnroll`)

## Folder layout
- `Features/`: Gherkin `.feature` files (plus Reqnroll-generated `.feature.cs`)
- `Steps/`: Reqnroll step bindings
- `Src/Core/`: config loader, driver factory, APK/activity resolvers
- `Src/Pages/`: page objects (`BasePage` holds shared waits/gestures)
- `Src/Support/`: lifecycle hooks, shared session state, step helpers
- `Configs/Profiles/`: execution profiles
- `Data/Login/`: test data
- `artifacts/screenshots/`: failure screenshots + page source
- `allure-results/`: Allure output

## Feature execution order
`Features/FeatureExecutionOrder.cs` pins the end-to-end order the Python suite
enforced via `conftest.py`:
Welcome -> WaitingList (RegistrationFlowWithWaitingListEnabled) -> Registration -> Login.

## Notes
- APK is resolved from the latest dated folder under:
  `C:/Users/akash.naykude/source/Shoppix android Build APKs`
- Expected APK filename: `Shoppix.apk`
- Real credentials must be provided via `.env` only (never commit real secrets).

## Local setup
1. Restore dependencies:
   `dotnet restore`
2. Create `.env` from `.env.example` and set at least:
   - `SHOPPIX_LOGIN_EMAIL`
   - `SHOPPIX_LOGIN_PASSWORD`
3. Start the Appium server on port 4723.
4. Connect an Android device via adb (USB debugging enabled); confirm with `adb devices`.
5. Ensure the `appium:udid` in `Configs/Profiles/stage.android.local.json` matches the device.

## Run commands
- Build:
  `dotnet build`
- List all discovered tests:
  `dotnet test --list-tests`
- Run the full suite:
  `dotnet test`
- Run only the Welcome feature:
  `dotnet test --filter "TestCategory=welcome"`
- Run a single scenario by name:
  `dotnet test --filter "Name=WelcomeScreenLoadsWithExpectedUIContent"`

Feature categories available for `--filter "TestCategory=..."`:
`welcome`, `waitlist_registration`, `registration`, `login`.
