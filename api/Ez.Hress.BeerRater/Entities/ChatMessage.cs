namespace Ez.Hress.BeerRater.Entities;

public class ChatMessage
{
    public int ID { get; set; }

    public int UserID { get; set; }

    public string Username { get; set; } = string.Empty;

    // The author's profile photo in the Hress image API, if they have one.
    public int? AuthorImageID { get; set; }

    // Plain text. Line breaks are kept as newlines.
    public string Text { get; set; } = string.Empty;

    public DateTime Created { get; set; }

    public DateTime? Edited { get; set; }
}
