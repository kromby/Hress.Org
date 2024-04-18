using Ez.Hress.Hardhead.Entities;

namespace Ez.Hress.Hardhead.UseCases;

public interface IRuleChangeDataAccess
{
    Task<int> SaveRuleChange(RuleChange ruleChange);

    Task<IList<RuleChange>> GetRuleChanges();

    Task<int> GetRuleChangeCount(int ruleID);
}
