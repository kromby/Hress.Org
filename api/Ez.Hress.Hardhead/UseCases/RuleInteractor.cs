using Ez.Hress.Hardhead.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.UseCases
{
    public class RuleInteractor
    {
        private readonly IRuleDataAccess _ruleDataAccess;
        private readonly IRuleChangeDataAccess _ruleChangeDataAccess;
        private readonly ILogger<RuleInteractor> _logger;
        private readonly string _className;

        public RuleInteractor(IRuleDataAccess ruleDataAccess, IRuleChangeDataAccess ruleChangeDataAccess, ILogger<RuleInteractor> logger)
        {
            _ruleDataAccess = ruleDataAccess;
            _ruleChangeDataAccess = ruleChangeDataAccess;
            _logger = logger;
            _className = nameof(RuleInteractor);
        }

        public async Task<int> SubmitRuleChange(RuleChange ruleChange)
        {
            string method = nameof(SubmitRuleChange);
            if (ruleChange == null)
                throw new ArgumentNullException(nameof(ruleChange));

            ruleChange.Validate();
            ruleChange.Inserted = DateTime.Now;

            _logger.LogInformation("[{Class}.{Method}] Rule type: '{typeID}' category: {categoryID}", _className, method, ruleChange.TypeID, ruleChange.RuleCategoryID);

            var result = await _ruleChangeDataAccess.SaveRuleChange(ruleChange);
            return result;
        }

        public async Task<IList<RuleChange>> GetRuleChanges()
        {
            string method = nameof(GetRuleChanges);
            _logger.LogInformation("[{Class}.{Method}] Getting rule changes.", _className, method);

            return await _ruleChangeDataAccess.GetRuleChanges(); 
        }

        public async Task<IList<RuleParent>> GetRules()
        {
            return await _ruleDataAccess.GetRules();
        }

        public async Task<IList<RuleChild>> GetRules(int parentID)
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
}
