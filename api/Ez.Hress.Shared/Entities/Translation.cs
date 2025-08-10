using Ez.Hress.Shared.Entities;

namespace Ez.Hress.Shared.Entities;

public class Translation
{
    public Translation(string sourceText, string translatedText, string sourceLanguage)
    {
        SourceText = sourceText;
        TranslatedText = translatedText;
        SourceLanguage = sourceLanguage;
        Inserted = DateTime.UtcNow;
    }

    public string SourceText { get; set; }
    public string TranslatedText { get; set; }
    public string SourceLanguage { get; set; }
    public DateTime Inserted { get; set; }
    public int InsertedBy { get; set; }
}
