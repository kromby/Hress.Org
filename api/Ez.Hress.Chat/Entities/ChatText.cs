using System.Net;
using System.Text.RegularExpressions;

namespace Ez.Hress.Chat.Entities;

// Chat messages are stored as HTML in scr_Text, like the legacy chat's FreeTextBox
// content. New messages are plain text, stored HTML-encoded with <br /> for line breaks.
public static partial class ChatText
{
    public const int MaxLength = 2000;

    [GeneratedRegex(@"<br\s*/?>", RegexOptions.IgnoreCase)]
    private static partial Regex LineBreak();

    // Trims a new message and checks its length. Throws ArgumentException for an invalid one.
    public static string Validate(string? text)
    {
        var trimmed = text?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
            throw new ArgumentException("Message text is required.", nameof(text));
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"Message text can be at most {MaxLength} characters.", nameof(text));

        return trimmed;
    }

    // Encodes a plain-text message for storage, so it's safe wherever it's shown as HTML.
    // Only HTML's special characters are encoded; WebUtility.HtmlEncode would also turn
    // Icelandic letters into numeric entities.
    public static string ToStoredHtml(string text) =>
        text.Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;")
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Replace("\n", "<br />");

    // Turns a stored message back into the plain text it was written as.
    // Other markup in legacy messages is left as literal text.
    public static string ToPlainText(string? html)
    {
        if (string.IsNullOrEmpty(html))
            return string.Empty;

        return WebUtility.HtmlDecode(LineBreak().Replace(html, "\n"));
    }
}
