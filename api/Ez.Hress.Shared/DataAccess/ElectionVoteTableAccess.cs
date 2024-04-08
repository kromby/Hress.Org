using Azure.Data.Tables;
using Ez.Hress.Shared.Entities;
using Ez.Hress.Shared.UseCases;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Shared.DataAccess
{
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
                VoteEntity vote = new(Guid.Parse(item.CategoryID), int.Parse(item.PartitionKey), item.Value);
                list.Add(vote);
            }

            return list;
        }
    }
}
