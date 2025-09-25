using Ez.Hress.Shared.Entities;

namespace Ez.Hress.Shared.Entities;

public class Translation
{
    public Translation(string sourceText, string translatedText, string sourceLanguage)
    {
        if (string.IsNullOrWhiteSpace(sourceText)) throw new ArgumentException("sourceText is required", nameof(sourceText));
        if (string.IsNullOrWhiteSpace(translatedText)) throw new ArgumentException("translatedText is required", nameof(translatedText));
        if (string.IsNullOrWhiteSpace(sourceLanguage)) throw new ArgumentException("sourceLanguage is required", nameof(sourceLanguage));
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
