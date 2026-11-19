using NUnit.Framework;

namespace Shoppix.MobileApp.Automation.Features;

// Forces the end-to-end feature execution order that the Python suite enforced
// via conftest.py's pytest_collection_modifyitems:
//   Welcome (10) -> WaitingList (20) -> Registration (30) -> Login (40).
// The feature classes are generated as `partial` by Reqnroll, so the NUnit
// [Order] attribute is attached here without touching the generated .feature.cs.

[Order(10)]
public partial class WelcomeScreenFeature
{
}

[Order(20)]
public partial class RegistrationFlowWithWaitingListEnabledFeature
{
}

[Order(30)]
public partial class RegistrationScreenFeature
{
}

[Order(40)]
public partial class LoginScreenFeature
{
}
