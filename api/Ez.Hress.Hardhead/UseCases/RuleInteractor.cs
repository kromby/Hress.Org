using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Shared.UseCases;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.Hardhead.UseCases;

public class RuleInteractor
{
    private readonly IRuleDataAccess _ruleDataAccess;
    private readonly IRuleChangeDataAccess _ruleChangeDataAccess;
    private readonly ITypeInteractor _typeInteractor;
    private readonly ILogger<RuleInteractor> _logger;
    private readonly string _className;

    public RuleInteractor(IRuleDataAccess ruleDataAccess, IRuleChangeDataAccess ruleChangeDataAccess, ITypeInteractor typeInteractor, ILogger<RuleInteractor> logger)
    {
        _ruleDataAccess = ruleDataAccess;
        _ruleChangeDataAccess = ruleChangeDataAccess;
        _typeInteractor = typeInteractor;
        _logger = logger;
        _className = nameof(RuleInteractor);
    }

    public async Task<int> SubmitRuleChangeAsync(RuleChange ruleChange)
    {
        string method = nameof(SubmitRuleChangeAsync);
        if (ruleChange == null)
            throw new ArgumentNullException(nameof(ruleChange));

        ruleChange.Validate();
        ruleChange.Inserted = DateTime.UtcNow;

        _logger.LogInformation("[{Class}.{Method}] Rule type: '{typeID}' category: {categoryID}", _className, method, ruleChange.TypeID, ruleChange.RuleCategoryID);

        var result = await _ruleChangeDataAccess.SaveRuleChange(ruleChange);
        return result;
    }

    public async Task<IList<RuleChange>> GetRuleChangesAsync(int typeID)
    {
        string method = nameof(GetRuleChangesAsync);
        _logger.LogInformation("[{Class}.{Method}] Getting rule changes.", _className, method);

        var allRuleChanges = await _ruleChangeDataAccess.GetRuleChanges();
        IList<RuleChange> filteredChanges = allRuleChanges.Where(c => c.TypeID == Enum.Parse<RuleChangeType>(typeID.ToString())).ToList();

        foreach(var entity in filteredChanges)
        {
            var type = await _typeInteractor.GetEzType((int)entity.TypeID);
            entity.TypeName = type.Name;
        }

        return filteredChanges;
    }

    public async Task<IList<RuleChange>> GetRuleChangesAsync()
    {
        string method = nameof(GetRuleChangesAsync);
        _logger.LogInformation("[{Class}.{Method}] Getting rule changes.", _className, method);

        var allRuleChanges = await _ruleChangeDataAccess.GetRuleChanges();

        foreach (var entity in allRuleChanges)
        {
            var type = await _typeInteractor.GetEzType((int)entity.TypeID);
            entity.TypeName = type.Name;
        }

        return allRuleChanges;
    }

    public async Task<IList<RuleChange>> GetRuleChangesByRuleAsync(int ruleID)
    {
        string method = nameof(GetRuleChangesAsync);
        _logger.LogInformation("[{Class}.{Method}] Getting rule changes ruleID: {RuleID}", _className, method, ruleID);

        var allRuleChanges = await _ruleChangeDataAccess.GetRuleChanges();
        IList<RuleChange> filteredChanges = allRuleChanges.Where(c => c.RuleID == ruleID).ToList();

        foreach (var entity in filteredChanges)
        {
            var type = await _typeInteractor.GetEzType((int)entity.TypeID);
            entity.TypeName = type.Name;
        }

        return filteredChanges;
    }

    public async Task<IList<RuleParent>> GetRulesAsync()
    {
        return await _ruleDataAccess.GetRules();
    }

    public async Task<IList<RuleChild>> GetRulesAsync(int parentID)
    {
        if (parentID < 1)
            throw new ArgumentException("Value can not be zero or negative.", nameof(parentID));

        var list = await _ruleDataAccess.GetRules(parentID);

        foreach (var rule in list)
        {
            rule.ChangeCount = await _ruleChangeDataAccess.GetRuleChangeCount(rule.ID);
        }

        return list;
    }
}
