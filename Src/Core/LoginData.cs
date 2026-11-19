using System.Text.Json;

namespace Shoppix.MobileApp.Automation.Core;

/// <summary>Typed view over Data/Login/stage_login_data.json.</summary>
public sealed class LoginData
{
    public string InvalidUserEmail { get; private set; } = "";
    public string InvalidUserPassword { get; private set; } = "";
    public string ExpectedInvalidTitle { get; private set; } = "";
    public List<string> ExpectedInvalidMessages { get; private set; } = new();

    public static LoginData Load(string path)
    {
        using var doc = ConfigLoader.LoadJson(path);
        var root = doc.RootElement;
        var data = new LoginData();

        if (root.TryGetProperty("invalid_user", out var invalid))
        {
            data.InvalidUserEmail = invalid.TryGetProperty("email", out var e) ? e.GetString() ?? "" : "";
            data.InvalidUserPassword = invalid.TryGetProperty("password", out var p) ? p.GetString() ?? "" : "";
        }

        if (root.TryGetProperty("expected", out var expected))
        {
            data.ExpectedInvalidTitle = expected.TryGetProperty("invalid_title", out var t) ? t.GetString() ?? "" : "";
            if (expected.TryGetProperty("invalid_messages", out var msgs) && msgs.ValueKind == JsonValueKind.Array)
            {
                data.ExpectedInvalidMessages = msgs.EnumerateArray()
                    .Select(m => m.GetString() ?? "")
                    .Where(m => m.Length > 0)
                    .ToList();
            }
        }

        return data;
    }
}
