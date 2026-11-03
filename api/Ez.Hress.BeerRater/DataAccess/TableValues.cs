using System.Globalization;
using Azure.Data.Tables;

namespace Ez.Hress.BeerRater.DataAccess;

// Reads properties from rows entered by hand in Storage Explorer, where a value
// may be stored as String instead of its proper type. Throws FormatException,
// naming the property, for a value that can't be converted.
internal static class TableValues
{
    public static string GetString(TableEntity entity, string key) =>
        entity.TryGetValue(key, out var value) ? value?.ToString() ?? string.Empty : string.Empty;

    public static string? GetOptionalString(TableEntity entity, string key)
    {
        var value = GetString(entity, key);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    public static double GetDouble(TableEntity entity, string key) =>
        GetValue(entity, key) switch
        {
            null => 0,
            double d => d,
            int i => i,
            long l => l,
            // Accept the Icelandic decimal comma as well as a point.
            string s when double.TryParse(s.Trim().Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var d) => d,
            var other => throw Invalid(key, other),
        };

    public static int GetInt32(TableEntity entity, string key) => GetOptionalInt32(entity, key) ?? 0;

    public static int? GetOptionalInt32(TableEntity entity, string key) =>
        GetValue(entity, key) switch
        {
            null => null,
            int i => i,
            long l when l is >= int.MinValue and <= int.MaxValue => (int)l,
            string s when int.TryParse(s.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) => i,
            var other => throw Invalid(key, other),
        };

    public static DateTime GetDateTime(TableEntity entity, string key) => GetOptionalDateTime(entity, key) ?? default;

    public static DateTime? GetOptionalDateTime(TableEntity entity, string key) =>
        GetValue(entity, key) switch
        {
            null => null,
            DateTimeOffset o => o.UtcDateTime,
            DateTime d => DateTime.SpecifyKind(d, DateTimeKind.Utc),
            string s when DateTimeOffset.TryParse(s.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var o) => o.UtcDateTime,
            var other => throw Invalid(key, other),
        };

    // A missing property and an empty string both count as no value.
    private static object? GetValue(TableEntity entity, string key) =>
        entity.TryGetValue(key, out var value) && !(value is string s && string.IsNullOrWhiteSpace(s)) ? value : null;

    private static FormatException Invalid(string key, object value) =>
        new($"Property '{key}' has a value that can't be read: '{value}' ({value.GetType().Name}).");
}
