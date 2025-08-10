using Ez.Hress.Shared.Entities;

namespace Ez.Hress.Shared.UseCases;

public interface ITranslationDataAccess
{
    /// <summary>
    /// Gets a cached translation for the specified text and source language.
    /// </summary>
    /// <param name="sourceText">The source text to look up</param>
    /// <param name="sourceLanguage">The source language code</param>
    /// <returns>The cached translation or null if not found</returns>
    Task<string?> GetTranslationAsync(string sourceText, string sourceLanguage);

    /// <summary>
    /// Saves a translation to the cache.
    /// </summary>
    /// <param name="translation">The translation entity to save</param>
    Task SaveTranslationAsync(Translation translation);

    /// <summary>
    /// Gets all translations for a specific source language.
    /// </summary>
    /// <param name="sourceLanguage">The source language code</param>
    /// <returns>List of translations for the specified language</returns>
    Task<IList<Translation>> GetTranslationsAsync(string sourceLanguage);
}
