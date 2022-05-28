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
    public class AwardTableDataAccess : IAwardDataAccess
    {
        private readonly ILogger<AwardTableDataAccess> _log;
        private readonly TableClient _tableClient;

        public AwardTableDataAccess(ILogger<AwardTableDataAccess> log, TableClient client)
        {
            _log = log;
            _tableClient = client;
        }
        
        public async void SaveNomination(Nomination nomination)
        {
            _log.LogInformation("Saving nomination");

            NominationTableEntity entity = new(nomination);
            await _tableClient.AddEntityAsync<NominationTableEntity>(entity);
        }
    }
}
