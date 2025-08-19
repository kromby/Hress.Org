using Ez.Hress.Shared.Entities;
using Ez.Hress.Shared.UseCases;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ez.Hress.Shared.UseCases;

public class TranslationService : ITranslationService
{
    private readonly ITranslationDataAccess _translationDataAccess;
    private readonly ILogger<TranslationService> _log;
    private readonly string _class = nameof(TranslationService);
    private readonly HttpClient _httpClient;

    public TranslationService(ITranslationDataAccess translationDataAccess, ILogger<TranslationService> log, HttpClient httpClient)
    {
        _translationDataAccess = translationDataAccess;
        _log = log;
        _httpClient = httpClient;
    }

    public async Task<string> TranslateAsync(string text, string sourceLanguage)
    {
        _log.LogInformation("[{Class}] TranslateAsync for language: {Language}", _class, sourceLanguage);

        // First, try to get from cache
        var cachedTranslation = await _translationDataAccess.GetTranslationAsync(text, sourceLanguage);
        if (!string.IsNullOrEmpty(cachedTranslation))
        {
            _log.LogInformation("[{Class}] Returning cached translation", _class);
            return cachedTranslation;
        }

        // If not in cache, translate using external service
        var translatedText = await TranslateWithExternalServiceAsync(text, sourceLanguage);
        
        // Save to cache (we'll need to create a Translation entity)
        var translation = new Translation(text, translatedText, sourceLanguage);
        await _translationDataAccess.SaveTranslationAsync(translation);

        _log.LogInformation("[{Class}] Translation completed and cached", _class);
        return translatedText;
    }

    public async Task<IList<string>> TranslateListAsync(IList<string> texts, string sourceLanguage)
    {
        _log.LogInformation("[{Class}] TranslateListAsync for {Count} texts, language: {Language}", 
            _class, texts.Count, sourceLanguage);

        var results = new List<string>();
        
        foreach (var text in texts)
        {
            var translatedText = await TranslateAsync(text, sourceLanguage);
            results.Add(translatedText);
        }

        return results;
    }

    public async Task<string?> GetCachedTranslationAsync(string text, string sourceLanguage)
    {
        _log.LogInformation("[{Class}] GetCachedTranslationAsync for language: {Language}", _class, sourceLanguage);
        return await _translationDataAccess.GetTranslationAsync(text, sourceLanguage);
    }

    public async Task SaveTranslationAsync(string sourceText, string translatedText, string sourceLanguage, int userId)
    {
        _log.LogInformation("[{Class}] SaveTranslationAsync for language: {Language}, userId: {UserId}", 
            _class, sourceLanguage, userId);

        var translation = new Translation(sourceText, translatedText, sourceLanguage)
        {
            InsertedBy = userId
        };
        
        await _translationDataAccess.SaveTranslationAsync(translation);
    }

    public async Task<string> TranslateWithExternalServiceAsync(string text, string sourceLanguage)
    {
        try
        {
            // Using Free Translate API which supports Icelandic
            // URL: https://ftapi.pythonanywhere.com/
            var encodedText = Uri.EscapeDataString(text);
            var url = $"https://ftapi.pythonanywhere.com/translate?sl={sourceLanguage}&dl=is&text={encodedText}";
            
            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseData = JsonSerializer.Deserialize<TranslationResponse>(responseContent);
                return responseData?.DestinationText ?? text; // Return original text if translation fails
            }
            else
            {
                _log.LogWarning("[{Class}] Translation service returned error: {StatusCode}", _class, response.StatusCode);
                return text; // Return original text if translation fails
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[{Class}] Error calling translation service", _class);
            return text; // Return original text if translation fails
        }
    }

    private class TranslationResponse
    {
        [JsonPropertyName("source-language")]
        public string SourceLanguage { get; set; } = string.Empty;
        [JsonPropertyName("source-text")]
        public string SourceText { get; set; } = string.Empty;
        [JsonPropertyName("destination-language")]
        public string DestinationLanguage { get; set; } = string.Empty;
        [JsonPropertyName("destination-text")]
        public string DestinationText { get; set; } = string.Empty;
        public Pronunciation? Pronunciation { get; set; }
        public Translations? Translations { get; set; }
        public List<Definition>? Definitions { get; set; }
        [JsonPropertyName("see-also")]
        public string? SeeAlso { get; set; }
    }

    private class Pronunciation
    {
        public string? SourceTextPhonetic { get; set; }
        public string? SourceTextAudio { get; set; }
        public string? DestinationTextAudio { get; set; }
    }

    private class Translations
    {
        public List<List<object>>? AllTranslations { get; set; }
        public List<string>? PossibleTranslations { get; set; }
        public List<string>? PossibleMistakes { get; set; }
    }

    private class Definition
    {
        public string PartOfSpeech { get; set; } = string.Empty;
        public string DefinitionText { get; set; } = string.Empty;
        public string? Example { get; set; }
        public List<string>? OtherExamples { get; set; }
        public Dictionary<string, List<string>>? Synonyms { get; set; }
    }
}
