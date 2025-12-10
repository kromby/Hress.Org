using Azure.Data.Tables;
using Ez.Hress.Shared.Entities;
using Ez.Hress.Shared.UseCases;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.Shared.DataAccess;

public class TranslationSqlDataAccess : ITranslationDataAccess
{
    private readonly TableClient _tableClient;
    private readonly ILogger<TranslationSqlDataAccess> _log;
    private readonly string _class = nameof(TranslationSqlDataAccess);

    public TranslationSqlDataAccess(BlobConnectionInfo connectionInfo, ILogger<TranslationSqlDataAccess> log)
    {
         var serviceClient = new TableServiceClient(connectionInfo.ConnectionString);
        serviceClient.CreateTableIfNotExists("Translations");
        _tableClient = new TableClient(connectionInfo.ConnectionString, "Translations");
        _log = log;
    }

    public async Task<string?> GetTranslationAsync(string sourceText, string sourceLanguage)
    {
        _log.LogInformation("[{Class}] GetTranslationAsync for language: {Language}", _class, sourceLanguage);
        
        try
        {
            var encodedSourceText = Uri.EscapeDataString(sourceText);
            var result = await _tableClient.GetEntityAsync<TranslationTableEntity>(sourceLanguage, encodedSourceText);
            
            if (result.HasValue)
            {
                _log.LogInformation("[{Class}] Found cached translation for text", _class);
                return result.Value.TranslatedText;
            }
            
            return null;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            _log.LogInformation("[{Class}] No cached translation found for text", _class);
            return null;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[{Class}] Error getting translation from cache", _class);
            return null;
        }
    }

    public async Task SaveTranslationAsync(Translation translation)
    {
        _log.LogInformation("[{Class}] SaveTranslationAsync for language: {Language}", _class, translation.SourceLanguage);
        
        try
        {
            var entity = new TranslationTableEntity(translation);
            await _tableClient.UpsertEntityAsync(entity);
            _log.LogInformation("[{Class}] Successfully saved translation to cache", _class);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[{Class}] Error saving translation to cache", _class);
            throw;
        }
    }

    public async Task<IList<Translation>> GetTranslationsAsync(string sourceLanguage)
    {
        _log.LogInformation("[{Class}] GetTranslationsAsync for language: {Language}", _class, sourceLanguage);
        
        try
        {
            var result = _tableClient.QueryAsync<TranslationTableEntity>(t => t.PartitionKey == sourceLanguage);
            
            var translations = new List<Translation>();
            await foreach (var entity in result)
            {
                var translation = new Translation(
                    Uri.UnescapeDataString(entity.RowKey), 
                    entity.TranslatedText, 
                    entity.SourceLanguage)
                {
                    Inserted = entity.Inserted,
                    InsertedBy = entity.InsertedBy
                };
                translations.Add(translation);
            }
            
            _log.LogInformation("[{Class}] Retrieved {Count} translations for language {Language}", 
                _class, translations.Count, sourceLanguage);
            
            return translations;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[{Class}] Error getting translations for language {Language}", _class, sourceLanguage);
            throw;
        }
    }
}
