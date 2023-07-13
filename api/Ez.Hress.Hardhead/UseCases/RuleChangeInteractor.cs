using Ez.Hress.Hardhead.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.UseCases
{
    public class RuleChangeInteractor
    {
        private readonly IRuleChangeDataAccess _ruleChangeDataAccess;
        private readonly ILogger<RuleChangeInteractor> _logger;

        public RuleChangeInteractor(IRuleChangeDataAccess ruleChangeDataAccess, ILogger<RuleChangeInteractor> logger)
        {
            _ruleChangeDataAccess = ruleChangeDataAccess;
            _logger = logger;
        }

        public async Task<int> SubmitRuleChange(RuleChange ruleChange)
        {
            if(ruleChange == null)
                throw new ArgumentNullException(nameof(ruleChange));

            ruleChange.Validate();
            ruleChange.Inserted = DateTime.Now;

            _logger.LogInformation("[{Class}.{Method}] Rule type: '{typeID}' category: {categoryID}", nameof(RuleChangeInteractor), nameof(SubmitRuleChange), ruleChange.TypeID, ruleChange.RuleCategoryID);

            var result = await _ruleChangeDataAccess.SaveRuleChange(ruleChange);
            return result;
        }
    }
}
