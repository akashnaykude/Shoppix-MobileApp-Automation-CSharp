using System.Text.Json;

namespace Shoppix.MobileApp.Automation.Core;

/// <summary>
/// Helpers to convert raw JSON into mutable .NET dictionaries/values.
/// Mirrors the dynamic dictionaries used by the Python config loader.
/// </summary>
public static class JsonConfig
{
    public static Dictionary<string, object?> ToDictionary(JsonElement obj)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var prop in obj.EnumerateObject())
        {
            dict[prop.Name] = ToValue(prop.Value);
        }
        return dict;
    }

    public static object? ToValue(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                return element.GetString();
            case JsonValueKind.True:
                return true;
            case JsonValueKind.False:
                return false;
            case JsonValueKind.Number:
                return element.TryGetInt64(out var l) ? l : element.GetDouble();
            case JsonValueKind.Object:
                return ToDictionary(element);
            case JsonValueKind.Array:
                return element.EnumerateArray().Select(ToValue).ToList();
            default:
                return null;
        }
    }
}
