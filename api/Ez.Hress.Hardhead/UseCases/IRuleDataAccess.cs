using Ez.Hress.Hardhead.Entities;

namespace Ez.Hress.Hardhead.UseCases;

public interface IRuleDataAccess
{
    Task<IList<RuleParent>> GetRules();

    Task<IList<RuleChild>> GetRules(int parentId);
}
