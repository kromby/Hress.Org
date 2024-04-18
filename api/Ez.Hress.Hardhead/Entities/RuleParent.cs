using Ez.Hress.Shared.Entities;

namespace Ez.Hress.Hardhead.Entities;

public class RuleParent : EntityBase<int>
{
    public int Number { get; set; }

    public HrefEntity SubRules
    {
        get => new()
{
Href = string.Format("/api/hardhead/rules/{0}", ID)
};
    }
}
