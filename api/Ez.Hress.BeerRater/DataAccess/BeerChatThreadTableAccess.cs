using Azure;
using Azure.Data.Tables;
using Ez.Hress.BeerRater.UseCases;
using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.BeerRater.DataAccess;

public class BeerChatThreadTableAccess : IBeerChatThreadDataAccess
{
    private readonly TableClient _threadsTable;
    private readonly ILogger<BeerChatThreadTableAccess> _logger;
    private readonly string _className;

    public BeerChatThreadTableAccess(BlobConnectionInfo connectionInfo, ILogger<BeerChatThreadTableAccess> logger)
    {
        _threadsTable = new TableClient(connectionInfo.ConnectionString, "BeerChatThreads");
        _threadsTable.CreateIfNotExists();
        _logger = logger;
        _className = nameof(BeerChatThreadTableAccess);
    }

    public async Task<int?> GetThreadIDAsync(string seasonID, string beerID)
    {
        _logger.LogInformation("[{Class}.{Method}] Get chat thread for beer '{BeerID}' in season '{SeasonID}'", _className, nameof(GetThreadIDAsync), beerID, seasonID);

        var response = await _threadsTable.GetEntityIfExistsAsync<ChatThreadTableEntity>(Uri.EscapeDataString(seasonID), Uri.EscapeDataString(beerID));
        return response.HasValue ? response.Value!.ThreadID : null;
    }

    public async Task<IDictionary<string, int>> GetThreadIDsAsync(string seasonID)
    {
        _logger.LogInformation("[{Class}.{Method}] Get chat threads for season '{SeasonID}'", _className, nameof(GetThreadIDsAsync), seasonID);

        var encodedSeasonID = Uri.EscapeDataString(seasonID);
        var threads = new Dictionary<string, int>(StringComparer.Ordinal);
        await foreach (var entity in _threadsTable.QueryAsync<ChatThreadTableEntity>(e => e.PartitionKey == encodedSeasonID))
        {
            threads[Uri.UnescapeDataString(entity.RowKey)] = entity.ThreadID;
        }

        return threads;
    }

    public async Task<bool> TryAddThreadAsync(string seasonID, string beerID, int threadID)
    {
        _logger.LogInformation("[{Class}.{Method}] Add chat thread {ThreadID} for beer '{BeerID}' in season '{SeasonID}'", _className, nameof(TryAddThreadAsync), threadID, beerID, seasonID);

        try
        {
            // AddEntity fails if the row exists, so two first posts can't both save a thread.
            await _threadsTable.AddEntityAsync(new ChatThreadTableEntity(seasonID, beerID, threadID));
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 409)
        {
            _logger.LogInformation("[{Class}.{Method}] Beer '{BeerID}' in season '{SeasonID}' already has a chat thread", _className, nameof(TryAddThreadAsync), beerID, seasonID);
            return false;
        }
    }
}
