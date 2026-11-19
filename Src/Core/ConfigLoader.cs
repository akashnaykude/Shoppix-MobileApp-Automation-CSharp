using System.Text.Json;

namespace Shoppix.MobileApp.Automation.Core;

public sealed class ApkConfig
{
    public string BaseFolder { get; set; } = "";
    public string LatestSubfolderBy { get; set; } = "modified_time";
    public string FixedApkName { get; set; } = "";
}

/// <summary>
/// Strongly typed view over a run profile JSON file
/// (configs/profiles/stage.android.local.json etc.).
/// </summary>
public sealed class RunProfile
{
    public string AppiumServer { get; set; } = "";
    public ApkConfig? Apk { get; set; }
    public Dictionary<string, object?> Capabilities { get; set; } = new();
}

/// <summary>Loads JSON configuration files. Mirrors src/core/config_loader.py.</summary>
public static class ConfigLoader
{
    public static JsonDocument LoadJson(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Config file missing: {path}");
        }

        return JsonDocument.Parse(File.ReadAllText(path));
    }

    public static RunProfile LoadProfile(string path)
    {
        using var doc = LoadJson(path);
        var root = doc.RootElement;

        var profile = new RunProfile
        {
            AppiumServer = root.TryGetProperty("appium_server", out var server)
                ? server.GetString() ?? ""
                : "",
        };

        if (root.TryGetProperty("apk", out var apk))
        {
            profile.Apk = new ApkConfig
            {
                BaseFolder = apk.TryGetProperty("base_folder", out var bf) ? bf.GetString() ?? "" : "",
                LatestSubfolderBy = apk.TryGetProperty("latest_subfolder_by", out var ls) ? ls.GetString() ?? "modified_time" : "modified_time",
                FixedApkName = apk.TryGetProperty("fixed_apk_name", out var fa) ? fa.GetString() ?? "" : "",
            };
        }

        if (root.TryGetProperty("capabilities", out var caps))
        {
            profile.Capabilities = JsonConfig.ToDictionary(caps);
        }

        return profile;
    }
}
