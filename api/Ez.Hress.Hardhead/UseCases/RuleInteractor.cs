using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Shared.UseCases;
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

        public async Task<IList<RuleChange>> GetRuleChanges(int typeID)
        {
            string method = nameof(GetRuleChanges);
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

        public async Task<IList<RuleChange>> GetRuleChanges()
        {
            string method = nameof(GetRuleChanges);
            _logger.LogInformation("[{Class}.{Method}] Getting rule changes.", _className, method);

            var allRuleChanges = await _ruleChangeDataAccess.GetRuleChanges();

            foreach (var entity in allRuleChanges)
            {
                var type = await _typeInteractor.GetEzType((int)entity.TypeID);
                entity.TypeName = type.Name;
            }

            return allRuleChanges;
        }

        public async Task<IList<RuleChange>> GetRuleChangesByRule(int ruleID)
        {
            string method = nameof(GetRuleChanges);
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
