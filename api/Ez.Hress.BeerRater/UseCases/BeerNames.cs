using System.Globalization;
using System.Text;

namespace Ez.Hress.BeerRater.UseCases;

public static class BeerNames
{
    // Folds a name for comparison: lower case, no diacritics, Icelandic letters
    // spelled out (ð -> d, þ -> th, æ -> ae) and whitespace collapsed.
    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        // These letters have no NFD decomposition, so map them first.
        var mapped = value.ToLowerInvariant()
            .Replace("ð", "d")
            .Replace("þ", "th")
            .Replace("æ", "ae");

        var builder = new StringBuilder(mapped.Length);
        foreach (var c in mapped.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(c);
            }
        }

        return string.Join(' ', builder.ToString().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}
