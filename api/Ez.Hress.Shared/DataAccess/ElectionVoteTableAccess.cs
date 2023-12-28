using Azure.Data.Tables;
using Ez.Hress.Shared.Entities;
using Ez.Hress.Shared.UseCases;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
            _tableClient = clients.Where(t => t.Name == "HardheadVotes").First();
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
    }
}
