namespace Ez.Hress.Shared.UseCases;

public interface ITranslationService
{
    /// <summary>
    /// Translates a single text from the specified source language to Icelandic.
    /// </summary>
    /// <param name="text">The text to translate</param>
    /// <param name="sourceLanguage">The source language code (e.g., "en")</param>
    /// <returns>The translated text in Icelandic</returns>
    Task<string> TranslateAsync(string text, string sourceLanguage);

    /// <summary>
    /// Translates a list of texts from the specified source language to Icelandic.
    /// </summary>
    /// <param name="texts">The list of texts to translate</param>
    /// <param name="sourceLanguage">The source language code (e.g., "en")</param>
    /// <returns>The list of translated texts in Icelandic</returns>
    Task<IList<string>> TranslateListAsync(IList<string> texts, string sourceLanguage);

    /// <summary>
    /// Gets a cached translation if available.
    /// </summary>
    /// <param name="text">The source text</param>
    /// <param name="sourceLanguage">The source language code</param>
    /// <returns>The cached translation or null if not found</returns>
    Task<string?> GetCachedTranslationAsync(string text, string sourceLanguage);

    /// <summary>
    /// Saves a translation to the cache.
    /// </summary>
    /// <param name="sourceText">The original text</param>
    /// <param name="translatedText">The translated text</param>
    /// <param name="sourceLanguage">The source language code</param>
    /// <param name="userId">The user ID who initiated the translation</param>
    Task SaveTranslationAsync(string sourceText, string translatedText, string sourceLanguage, int userId);
}
