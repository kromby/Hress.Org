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
    public class AwardNominateTableDataAccess : IAwardNominateDataAccess, IAwardNominationDataAccess
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
            _log.LogInformation("[{Class}] Saving nomination", this.GetType().Name);

            NominationTableEntity entity = new(nomination);
            await _tableClient.AddEntityAsync<NominationTableEntity>(entity);
            return 1;
        }

        public Task<IList<Nomination>> GetNominations(int typeID)
        {
            var result = _tableClient.Query<NominationTableEntity>(x => x.PartitionKey == typeID.ToString());
            IList<Nomination> list = new List<Nomination>();

            foreach(var entity in result)
            {
                var nomination = new Nomination(int.Parse(entity.PartitionKey), entity.NomineeID, entity.Description) { InsertedBy = entity.CreatedBy };
                nomination.ID = entity.RowKey;
                nomination.Nominee.Name = entity.NomineeName;
                if(entity.NomineeImageID.HasValue)
                    nomination.Nominee.ProfilePhotoId = entity.NomineeImageID.Value;                
                if (entity.Timestamp.HasValue)
                    nomination.Inserted = entity.Timestamp.Value.LocalDateTime;
                list.Add(nomination);
            }

            return Task.FromResult(list);
        }
    }
}
