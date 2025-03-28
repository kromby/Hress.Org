﻿using Azure.Data.Tables;
using Ez.Hress.Shared.Entities;
using Ez.Hress.Shared.UseCases;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.Shared.DataAccess;

public class ElectionVoteTableAccess : IElectionVoteDataAccess
{
    private readonly TableClient _tableClient;
    private readonly ILogger<ElectionVoteTableAccess> _log;

    public ElectionVoteTableAccess(IList<TableClient> clients, ILogger<ElectionVoteTableAccess> log)
    {
        _tableClient = clients.First(t => t.Name == "HardheadVotes");
        _log = log;
    }

    public async Task<bool> SaveVote(VoteEntity vote)
    {
        _log.LogInformation("[{Class}] SaveVote", this.GetType().Name);

        VoteTableEntity entity = new(vote);
        var result = await _tableClient.UpsertEntityAsync<VoteTableEntity>(entity);
        if(result.IsError)
        {
            _log.LogError("[{Class}] SaveVote status: {Status} error: {Error}", this.GetType().Name, result.Status, result.ReasonPhrase);
            return false;
        }
        return true;
    }

    public async Task<IList<VoteEntity>> GetVotes(Guid id)
    {
        _log.LogInformation("[{Class}] GetVotes", this.GetType().Name);

        var tableList = _tableClient.QueryAsync<VoteTableEntity>(t => t.CategoryID == id.ToString());
        var list = new List<VoteEntity>();
        await foreach (var item in tableList)
        {
            // skipcq: CS-R1004
            VoteEntity vote = new(Guid.Parse(item.CategoryID), int.Parse(item.PartitionKey), item.Value);
            list.Add(vote);
        }

        return list;
    }
}
