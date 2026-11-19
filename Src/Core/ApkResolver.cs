namespace Shoppix.MobileApp.Automation.Core;

/// <summary>Resolves the latest APK under a dated build folder. Mirrors src/core/apk_resolver.py.</summary>
public static class ApkResolver
{
    public static string ResolveLatestApk(string baseFolder, string apkName)
    {
        if (!Directory.Exists(baseFolder))
        {
            throw new DirectoryNotFoundException($"APK base folder not found: {baseFolder}");
        }

        var subfolders = Directory.GetDirectories(baseFolder);
        if (subfolders.Length == 0)
        {
            throw new FileNotFoundException($"No dated folders found under: {baseFolder}");
        }

        var latest = subfolders
            .OrderByDescending(p => Directory.GetLastWriteTimeUtc(p))
            .First();

        var apkPath = Path.Combine(latest, apkName);
        if (!File.Exists(apkPath))
        {
            throw new FileNotFoundException($"APK not found in latest folder: {apkPath}");
        }

        return apkPath;
    }
}
