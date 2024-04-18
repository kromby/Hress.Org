using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Shared.UseCases;
using System.Data;
using System.Text;

namespace Ez.Hress.Hardhead.UseCases;

public class PostElectionInteractor
{
    private readonly RuleInteractor _ruleInteractor;
    private readonly IElectionVoteDataAccess _voteDataAccess;

    public PostElectionInteractor(RuleInteractor ruleInteractor, IElectionVoteDataAccess voteDataAccess)
    {
        _ruleInteractor = ruleInteractor;
        _voteDataAccess = voteDataAccess;
    }


    public async Task<string> UpdateRulesAsync()
    {
        var list = await _ruleInteractor.GetRuleChangesAsync();
        var newRuleList = list.Where(rc => rc.TypeID == RuleChangeType.Create);
        var newRuleTask = HandleNewRulesAsync(newRuleList.ToList());

        var changedRuleList = list.Where(rc => rc.TypeID == RuleChangeType.Update || rc.TypeID == RuleChangeType.Delete);
        var changedRuleTask = HandleChangedRulesAsync(changedRuleList.ToList());

        newRuleTask.Wait();
        string newRules = newRuleTask.Result;

        changedRuleTask.Wait();
        string changedRules = changedRuleTask.Result;

        return $"{newRules}{changedRules}";

    }

    private async Task<string> HandleNewRulesAsync(IList<RuleChange> ruleChanges)
    {
        StringBuilder sb = new();
        foreach (var rule in ruleChanges)
        {
            if (rule.ID == null)
                continue;

            var votes = await _voteDataAccess.GetVotes(Guid.Parse(rule.ID));

            int approved = votes.Count(v => v.Value == "1");
            int declined = votes.Count(v => v.Value == "-1");

            if (approved > declined)
            {
                sb.AppendLine($"INSERT INTO rep_Text(EventID, TypeID, ParentID, TextValue, InsertedBy) VALUE(4782, 70, {rule.RuleCategoryID}, {rule.RuleText}, 2630)");
            }
        }
        return sb.ToString();
    }

    private async Task<string> HandleChangedRulesAsync(IList<RuleChange> ruleChanges)
    {
        StringBuilder sb = new();

        var ids = ruleChanges.Select(t => t.RuleID).ToList();
        foreach(var id in ids)
        {
            var ruleList = ruleChanges.Where(t => t.RuleID == id).ToList();

            if(ruleList.Count == 1)
            {
                RuleChange rule = ruleList.First();
                if (rule.ID == null)
                    continue;

                var votes = await _voteDataAccess.GetVotes(Guid.Parse(rule.ID));

                int approved = votes.Count(v => v.Value == "1");
                int declined = votes.Count(v => v.Value == "-1");

                if (approved > declined)
                {
                    if(rule.TypeID == RuleChangeType.Update)
                        sb.AppendLine($"UPDATE [dbo].[rep_Text] SET [TextValue]='{rule.RuleText}', [Updated]='{DateTime.UtcNow.ToString("yyyy-MM-dd")}', [UpdatedBy]=2630 WHERE Id={rule.RuleID}");
                    else if(rule.TypeID == RuleChangeType.Delete)
                        sb.AppendLine($"UPDATE [dbo].[rep_Text] SET Deleted='{DateTime.UtcNow.ToString("yyyy-MM-dd")}' WHERE Id={rule.RuleID}");
                }
            }
            else
            {
                sb.AppendLine("MULTI OPTION NOT HANDLED");
            }
        }

        return sb.ToString();
    }
}
