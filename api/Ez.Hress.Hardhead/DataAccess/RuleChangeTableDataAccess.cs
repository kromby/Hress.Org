using Azure.Data.Tables;
using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.Hardhead.DataAccess;

public class RuleChangeTableDataAccess : IRuleChangeDataAccess
{
    private readonly TableClient _tableClient;
    private readonly ILogger<RuleChangeTableDataAccess> _logger;
    private readonly string _className;

    public RuleChangeTableDataAccess(BlobConnectionInfo connectionInfo, ILogger<RuleChangeTableDataAccess> logger)
    {
        _tableClient = new TableClient(connectionInfo.ConnectionString, "HardheadRuleChanges");
        _logger = logger;
        _className = nameof(RuleChangeTableDataAccess);
    }        

    public async Task<IList<RuleChange>> GetRuleChanges()
    {
        string method = nameof(GetRuleChanges);
        _logger.LogInformation("[{Class}.{Method}] Get rule changes", _className, method);

        var result = _tableClient.QueryAsync<RuleChangeTableEntity>();
        IList<RuleChange> list = new List<RuleChange>();

        await foreach (var entity in result)
        {
            var rulaChange = new RuleChange(Enum.Parse<RuleChangeType>(entity.PartitionKey), entity.RuleCategoryID, entity.Reasoning)
            {
                Inserted = entity.Timestamp.HasValue ? entity.Timestamp.Value.LocalDateTime : DateTime.Today,
                InsertedBy = entity.CreatedBy,
                ID = entity.RowKey,
                RuleText = entity.RuleText,
                RuleID = entity.RuleID,
                Name = entity.Name
            };
            list.Add(rulaChange);
        }

        return list;
    }

    public async Task<int> SaveRuleChange(RuleChange ruleChange)
    {
        string method = nameof(SaveRuleChange);
        _logger.LogInformation("[{Class}.{Method}] Saving rule change", _className, method);

        RuleChangeTableEntity entity = new(ruleChange);
        var response = await _tableClient.AddEntityAsync<RuleChangeTableEntity>(entity);
        return response.IsError ? 0 : 1;
    }

    public async Task<int> GetRuleChangeCount(int ruleID)
    {
        string method = nameof(GetRuleChangeCount);
        _logger.LogInformation("[{Class}.{Method}] Get rule change count for rule {ruleID}", _className, method, ruleID);

        var result = _tableClient.QueryAsync<RuleChangeTableEntity>(rc => (rc.PartitionKey == "Update" || rc.PartitionKey == "Delete") && rc.RuleID == ruleID);

        int count = 0;
        await foreach(var entity in result)
        {
            count++;
        }

        return count;
    }
}
