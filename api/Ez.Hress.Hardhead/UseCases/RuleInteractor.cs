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
        private readonly ILogger<RuleInteractor> _logger;
        private readonly string _className;

        public RuleInteractor(IRuleDataAccess ruleDataAccess, ILogger<RuleInteractor> logger)
        {
            _ruleDataAccess = ruleDataAccess;
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

            var result = await _ruleDataAccess.SaveRuleChange(ruleChange);
            return result;
        }

        public async Task<IList<RuleChange>> GetRuleChanges()
        {
            string method = nameof(GetRuleChanges);
            _logger.LogInformation("[{Class}.{Method}] Getting rule changes.", _className, method);

            return await _ruleDataAccess.GetRuleChanges(); 
        }
    }
}
