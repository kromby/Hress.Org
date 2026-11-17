namespace Ez.Hress.Chat.Entities;

public class ChatMessage
{
    public int ID { get; set; }

    public int ThreadID { get; set; }

    public int UserID { get; set; }

    public string Username { get; set; } = string.Empty;

    // The author's profile photo in the image API, if they have one.
    public int? AuthorImageID { get; set; }

    public string Text { get; set; } = string.Empty;

    public DateTime Inserted { get; set; }

    public DateTime? Updated { get; set; }
}
