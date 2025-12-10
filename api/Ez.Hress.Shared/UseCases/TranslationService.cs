using Ez.Hress.Shared.Entities;
using Ez.Hress.Shared.UseCases;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Concurrent;
using System.Threading;

namespace Ez.Hress.Shared.UseCases;

public class TranslationService
{
    private readonly ITranslationDataAccess _translationDataAccess;
    private readonly ILogger<TranslationService> _log;
    private readonly string _class = nameof(TranslationService);
    private readonly HttpClient _httpClient;
    
    // In-memory cache for frequently used translations
    private readonly ConcurrentDictionary<string, (string Value, DateTime Added)> _memoryCache = new();
    private readonly object _cacheLock = new object();
    private DateTime _lastCacheCleanup = DateTime.UtcNow;
    private const int CACHE_CLEANUP_INTERVAL_MINUTES = 30;
    private const int MAX_CACHE_SIZE = 1000;

    public TranslationService(ITranslationDataAccess translationDataAccess, ILogger<TranslationService> log, HttpClient httpClient)
    {
        _translationDataAccess = translationDataAccess;
        _log = log;
        _httpClient = httpClient;
    }

    public async Task<string> TranslateAsync(string text, string sourceLanguage, CancellationToken cancellationToken = default)
    {
        _log.LogInformation("[{Class}] TranslateAsync for language: {Language}", _class, sourceLanguage);

        // First, try in-memory cache
        var cacheKey = $"{sourceLanguage}:{text}";
        if (_memoryCache.TryGetValue(cacheKey, out var memoryCached))
        {
            _log.LogInformation("[{Class}] Returning in-memory cached translation", _class);
            return memoryCached.Value;
        }

        // Then, try database cache
        var cachedTranslation = await _translationDataAccess.GetTranslationAsync(text, sourceLanguage);
        if (!string.IsNullOrEmpty(cachedTranslation))
        {
            _log.LogInformation("[{Class}] Returning database cached translation", _class);
            
            // Add to in-memory cache
            AddToMemoryCache(cacheKey, cachedTranslation);
            return cachedTranslation;
        }

        // If not in cache, translate using external service
        var translatedText = await TranslateWithExternalServiceAsync(text, sourceLanguage, cancellationToken);
        
        if(translatedText == text)
        {
            _log.LogWarning("[{Class}] Translation failed for text: {Text}", _class, text);
            return text;
        }

        // Save to both caches
        var translation = new Translation(text, translatedText, sourceLanguage);
        await _translationDataAccess.SaveTranslationAsync(translation);
        AddToMemoryCache(cacheKey, translatedText);

        _log.LogInformation("[{Class}] Translation completed and cached", _class);
        return translatedText;
    }

    public async Task<IList<string>> TranslateListAsync(IList<string> texts, string sourceLanguage, CancellationToken cancellationToken = default)
    {
        _log.LogInformation("[{Class}] TranslateListAsync for language: {Language}", _class, sourceLanguage);
        
        var translations = new List<string>();
        foreach (var text in texts)
        {
            var translatedText = await TranslateAsync(text, sourceLanguage, cancellationToken);
            translations.Add(translatedText);
        }
        return translations;
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

    public async Task<string> TranslateWithExternalServiceAsync(string text, string sourceLanguage, CancellationToken cancellationToken = default)
    {
        try
        {
            // Using Free Translate API which supports Icelandic
            // URL: https://ftapi.pythonanywhere.com/
            var encodedText = Uri.EscapeDataString(text);
            var url = $"https://ftapi.pythonanywhere.com/translate?sl={sourceLanguage}&dl=is&text={encodedText}";
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var responseData = JsonSerializer.Deserialize<TranslationResponse>(responseContent);
                return responseData?.DestinationText ?? text; // Return original text if translation fails
            }
                
            _log.LogWarning("[{Class}] Translation service returned error: {StatusCode}", _class, response.StatusCode);
            return text; // Return original text if translation fails
        }
        catch (TaskCanceledException)
        {
            _log.LogWarning("[{Class}] Translation request was cancelled", _class);
            return text;
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

    private void AddToMemoryCache(string cacheKey, string translation)
    {
        lock (_cacheLock)
        {
            // Cleanup cache if it's getting too large or time has passed
            if (_memoryCache.Count >= MAX_CACHE_SIZE || 
                DateTime.UtcNow.Subtract(_lastCacheCleanup).TotalMinutes >= CACHE_CLEANUP_INTERVAL_MINUTES)
            {
                CleanupMemoryCache();
            }
            
            _memoryCache.TryAdd(cacheKey, (translation, DateTime.UtcNow));
        }
    }

    private void CleanupMemoryCache()
    {
        // Remove oldest entries (simple FIFO cleanup)
        var keysToRemove = _memoryCache
            .OrderBy(kvp => kvp.Value.Added)
            .Take(_memoryCache.Count / 2)
            .Select(kvp => kvp.Key)
            .ToList();
        foreach (var key in keysToRemove)
        {
            _memoryCache.TryRemove(key, out _);
        }
        
        _lastCacheCleanup = DateTime.UtcNow;
        _log.LogInformation("[{Class}] Cleaned up memory cache, removed {Count} entries", _class, keysToRemove.Count);
    }
}
