using Azure.Data.Tables;
using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.DataAccess
{
    public class RuleChangeTableDataAccess : IRuleChangeDataAccess
    {
        private readonly TableClient _tableClient;
        private readonly ILogger<RuleChangeTableDataAccess> _logger;
        
        public RuleChangeTableDataAccess(BlobConnectionInfo connectionInfo, ILogger<RuleChangeTableDataAccess> logger)
        {
            _tableClient = new TableClient(connectionInfo.ConnectionString, "HardheadRuleChanges");
            _logger = logger;
        }

        public async Task<int> SaveRuleChange(RuleChange ruleChange)
        {
            _logger.LogInformation("[{Class}] Saving rule change", this.GetType().Name);

            RuleChangeTableEntity entity = new(ruleChange);
            var response = await _tableClient.AddEntityAsync<RuleChangeTableEntity>(entity);
            return response.IsError ? 0 : 1;
        }
    }
}
