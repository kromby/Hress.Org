using Azure;
using Azure.Data.Tables;
using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.DataAccess
{
    public class AwardNominateTableDataAccess : IAwardNominateDataAccess, IAwardNominationDataAccess
    {
        private readonly ILogger<AwardNominateTableDataAccess> _logger;
        private readonly TableClient _tableClient;

        public AwardNominateTableDataAccess(BlobConnectionInfo connectionInfo, ILogger<AwardNominateTableDataAccess> log)
        {
            _logger = log;
            _tableClient = new TableClient(connectionInfo.ConnectionString, "HardheadNominations");
        }

        public async Task<int> SaveNomination(Nomination nomination)
        {
            _logger.LogInformation("[{Class}] Saving nomination", this.GetType().Name);

            NominationTableEntity entity = new(nomination);
            var response = await _tableClient.AddEntityAsync<NominationTableEntity>(entity);
            return response.IsError ? 0 : 1;
        }

        public Task<IList<Nomination>> GetNominations(int typeID)
        {
            var result = _tableClient.Query<NominationTableEntity>(x => x.PartitionKey == typeID.ToString());
            IList<Nomination> list = new List<Nomination>();

            foreach(var entity in result)
            {
                var nomination = new Nomination(int.Parse(entity.PartitionKey), entity.NomineeID, entity.Description)
                {
                    InsertedBy = entity.CreatedBy,
                    ID = entity.RowKey
                };
                nomination.Nominee.Name = entity.NomineeName;
                if(entity.NomineeImageID.HasValue)
                    nomination.Nominee.ProfilePhotoId = entity.NomineeImageID.Value;                
                if (entity.Timestamp.HasValue)
                    nomination.Inserted = entity.Timestamp.Value.LocalDateTime;
                nomination.AffectedRule = entity.AffectedRule;
                list.Add(nomination);
            }

            return Task.FromResult(list);
        }
    }
}
