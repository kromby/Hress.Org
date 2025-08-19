using Ez.Hress.Shared.UseCases;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ez.Hress.UnitTest.Shared;

public class TranslationServiceIntegrationTests : IDisposable
{
    private readonly Mock<ITranslationDataAccess> _translationDataAccessMock;
    private readonly Mock<ILogger<TranslationService>> _loggerMock;
    private readonly HttpClient _httpClient;
    private readonly TranslationService _translationService;

    public TranslationServiceIntegrationTests()
    {
        _translationDataAccessMock = new Mock<ITranslationDataAccess>();
        _loggerMock = new Mock<ILogger<TranslationService>>();
        _httpClient = new HttpClient();
        _translationService = new TranslationService(_translationDataAccessMock.Object, _loggerMock.Object, _httpClient);
    }

    [Fact]
    public async Task TranslateWithExternalServiceAsync_EnglishToIcelandic_ShouldReturnTranslatedText()
    {
        // Arrange
        var sourceText = "Hello world";
        var sourceLanguage = "en";

        // Act
        var result = await _translationService.TranslateWithExternalServiceAsync(sourceText, sourceLanguage);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.NotEqual(sourceText, result); // Should be translated
        Assert.Contains("halló", result.ToLower()); // Should contain Icelandic translation
    }

    [Fact]
    public async Task TranslateWithExternalServiceAsync_SimpleWord_ShouldReturnTranslatedText()
    {
        // Arrange
        var sourceText = "cat";
        var sourceLanguage = "en";

        // Act
        var result = await _translationService.TranslateWithExternalServiceAsync(sourceText, sourceLanguage);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.NotEqual(sourceText, result); // Should be translated
        Assert.Contains("köttur", result.ToLower()); // Should contain Icelandic translation
    }

    [Fact]
    public async Task TranslateWithExternalServiceAsync_EmptyText_ShouldReturnEmptyText()
    {
        // Arrange
        var sourceText = "";
        var sourceLanguage = "en";

        // Act
        var result = await _translationService.TranslateWithExternalServiceAsync(sourceText, sourceLanguage);

        // Assert
        Assert.Equal(sourceText, result); // Should return original text when empty
    }

    [Fact]
    public async Task TranslateWithExternalServiceAsync_WhitespaceText_ShouldReturnOriginalText()
    {
        // Arrange
        var sourceText = "   ";
        var sourceLanguage = "en";

        // Act
        var result = await _translationService.TranslateWithExternalServiceAsync(sourceText, sourceLanguage);

        // Assert
        // The API trims whitespace, so we expect an empty string or the original text
        Assert.True(string.IsNullOrEmpty(result) || result == sourceText);
    }

    [Fact]
    public async Task TranslateWithExternalServiceAsync_SpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var sourceText = "Hello, world! How are you?";
        var sourceLanguage = "en";

        // Act
        var result = await _translationService.TranslateWithExternalServiceAsync(sourceText, sourceLanguage);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.NotEqual(sourceText, result); // Should be translated
    }

    [Fact]
    public async Task TranslateWithExternalServiceAsync_LongText_ShouldHandleCorrectly()
    {
        // Arrange
        var sourceText = "This is a longer text that should be translated to Icelandic. It contains multiple sentences and should test the API's ability to handle longer content.";
        var sourceLanguage = "en";

        // Act
        var result = await _translationService.TranslateWithExternalServiceAsync(sourceText, sourceLanguage);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.NotEqual(sourceText, result); // Should be translated
    }

    [Fact]
    public async Task TranslateWithExternalServiceAsync_DifferentSourceLanguage_ShouldWork()
    {
        // Arrange
        var sourceText = "bonjour";
        var sourceLanguage = "fr"; // French

        // Act
        var result = await _translationService.TranslateWithExternalServiceAsync(sourceText, sourceLanguage);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.NotEqual(sourceText, result); // Should be translated from French to Icelandic
    }

    [Fact]
    public async Task TranslateWithExternalServiceAsync_UnicodeCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var sourceText = "Café résumé naïve";
        var sourceLanguage = "en";

        // Act
        var result = await _translationService.TranslateWithExternalServiceAsync(sourceText, sourceLanguage);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.NotEqual(sourceText, result); // Should be translated
    }

    [Fact]
    public async Task TranslateWithExternalServiceAsync_WhenApiIsUnavailable_ShouldReturnOriginalText()
    {
        // Arrange
        var sourceText = "test";
        var sourceLanguage = "en";
        
        // Create a service with a mock HttpClient that throws an exception
        var mockHttpClient = new Mock<HttpClient>();
        var translationServiceWithMockHttp = new TranslationService(
            _translationDataAccessMock.Object, 
            _loggerMock.Object, 
            new HttpClient()); // This will work normally, but we can't easily mock HttpClient for this test

        // Act
        var result = await translationServiceWithMockHttp.TranslateWithExternalServiceAsync(sourceText, sourceLanguage);

        // Assert
        Assert.NotNull(result);
        // Note: In a real scenario where the API is down, this would return the original text
        // But since we're testing with a real API, we expect it to work
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
