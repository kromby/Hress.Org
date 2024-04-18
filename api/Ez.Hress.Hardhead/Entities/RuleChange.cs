using Ez.Hress.Shared.Entities;

namespace Ez.Hress.Hardhead.Entities;

public class RuleChange : EntityBase<string>
{
    public RuleChange(RuleChangeType typeID, int ruleCategoryID, string reasoning)
    {
        TypeID = typeID;
        RuleCategoryID = ruleCategoryID;
        Reasoning = reasoning;
    }

    public RuleChangeType TypeID { get; set; }
    public string? TypeName { get; set; }

    public int RuleCategoryID { get; set; }
    public int? RuleID { get; set; }
    public string? RuleText { get; set; }

    public string Reasoning { get; set; }

    public void Validate()
    {
        if (RuleCategoryID <= 0)
            throw new ArgumentException("Must be greater then zero", nameof(RuleCategoryID));
        if (InsertedBy <= 0)
            throw new ArgumentException("CreatedBy must be larger then zero", nameof(InsertedBy));
        if (string.IsNullOrWhiteSpace(Reasoning))
            throw new ArgumentNullException(nameof(Reasoning));

        switch (TypeID)
        {
            case RuleChangeType.Update:
                if (string.IsNullOrWhiteSpace(RuleText) || RuleText.Length < 16)
                    throw new ArgumentException("Must be at least 16 characters", nameof(RuleText));
                break;
            case RuleChangeType.Create:
                if (string.IsNullOrWhiteSpace(RuleText) || RuleText.Length < 16)
                    throw new ArgumentException("Must be at least 16 characters", nameof(RuleText));
                break;
            case RuleChangeType.Delete:
                break;
            default:
                throw new ArgumentException($"Type '{TypeID}' not supported", nameof(TypeID));
        }

    }
}

public enum RuleChangeType
{
    None = 0,
    Update = 210,
    Create = 209,
    Delete = 211
}
