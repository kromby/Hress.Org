using System.Net;
using System.Text.RegularExpressions;

namespace Ez.Hress.Chat.Entities;

// Chat messages are stored as HTML in scr_Text, like the legacy chat's FreeTextBox
// content. New messages are plain text, stored HTML-encoded with <br /> for line breaks.
public static partial class ChatText
{
    [GeneratedRegex(@"<br\s*/?>", RegexOptions.IgnoreCase)]
    private static partial Regex LineBreak();

    // Turns a stored message back into the plain text it was written as.
    // Other markup in legacy messages is left as literal text.
    public static string ToPlainText(string? html)
    {
        if (string.IsNullOrEmpty(html))
            return string.Empty;

        return WebUtility.HtmlDecode(LineBreak().Replace(html, "\n"));
    }
}
