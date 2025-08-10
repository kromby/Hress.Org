using Azure;
using Azure.Data.Tables;
using Ez.Hress.Shared.Entities;
using System.Web;

namespace Ez.Hress.Shared.DataAccess;

internal class TranslationTableEntity : ITableEntity
{
    public TranslationTableEntity()
    {
        PartitionKey = string.Empty;
        RowKey = string.Empty;
        TranslatedText = string.Empty;
        SourceLanguage = string.Empty;
    }

    public TranslationTableEntity(Translation translation)
    {
        PartitionKey = translation.SourceLanguage;
        RowKey = HttpUtility.UrlEncode(translation.SourceText);
        TranslatedText = translation.TranslatedText;
        SourceLanguage = translation.SourceLanguage;
        Inserted = translation.Inserted;
        InsertedBy = translation.InsertedBy;
    }

    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string TranslatedText { get; set; }
    public string SourceLanguage { get; set; }
    public DateTime Inserted { get; set; }
    public int InsertedBy { get; set; }
}
