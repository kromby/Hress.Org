using Azure;
using Azure.Data.Tables;
using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Hardhead.UseCases;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.DataAccess
{
    public class AwardNominateTableDataAccess : IAwardNominateDataAccess
    {
        private readonly ILogger<AwardNominateTableDataAccess> _log;
        private readonly TableClient _tableClient;

        public AwardNominateTableDataAccess(TableClient client, ILogger<AwardNominateTableDataAccess> log)
        {
            _log = log;
            _tableClient = client;
        }
        
        public async Task<int> SaveNomination(Nomination nomination)
        {
            _log.LogInformation("[AwardTableDataAccess] Saving nomination");

            NominationTableEntity entity = new(nomination);
            await _tableClient.AddEntityAsync<NominationTableEntity>(entity);
            return 1;
        }
    }
}
