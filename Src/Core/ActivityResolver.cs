using System.Diagnostics;

namespace Shoppix.MobileApp.Automation.Core;

/// <summary>
/// Resolves the current launchable activity for a package from a connected
/// device via adb. Mirrors src/core/activity_resolver.py.
/// </summary>
public static class ActivityResolver
{
    public static string? ResolveLaunchActivity(string udid, string appPackage)
    {
        string output;
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "adb",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            foreach (var arg in new[]
                     {
                         "-s", udid, "shell", "cmd", "package",
                         "resolve-activity", "--brief", appPackage,
                     })
            {
                psi.ArgumentList.Add(arg);
            }

            using var process = Process.Start(psi);
            if (process is null)
            {
                return null;
            }

            output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
            process.WaitForExit(15000);
        }
        catch
        {
            return null;
        }

        var lines = output
            .Split('\n')
            .Select(l => l.Trim())
            .Where(l => l.Length > 0)
            .ToList();

        for (var i = lines.Count - 1; i >= 0; i--)
        {
            var line = lines[i];
            if (line.Contains('/') && line.StartsWith(appPackage, StringComparison.Ordinal))
            {
                var parts = line.Split('/', 2);
                if (parts.Length == 2)
                {
                    return parts[1];
                }
            }
        }

        return null;
    }
}
