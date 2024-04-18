using Ez.Hress.Shared.Entities;

namespace Ez.Hress.Hardhead.Entities;

public class RuleChild : EntityBase<int>
{
    public HrefEntity? Changes
    {
        get
        {
            if (ChangeCount == 0)
                return null;

            return new HrefEntity()
            {
                Href = string.Format("/api/hardhead/rules/{0}/changes", ID)
            };
        }
    }

    public int ParentNumber { get; set; }
    public int Number { get; set; }
    public int ChangeCount { private get; set; }
}
