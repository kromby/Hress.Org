using Ez.Hress.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.Entities
{
    public class RuleChange : EntityBase<string>
    {
        public RuleChange(RuleChangeType typeID, int ruleCategoryID) {
            TypeID = typeID;
            RuleCategoryID = ruleCategoryID;
        }

        public RuleChangeType TypeID { get; set; }

        public int RuleCategoryID { get; set; }

        public string? RuleText { get; set; }

        public void Validate()
        {
            if (RuleCategoryID <= 0)
                throw new ArgumentException("Must be greater then zero", nameof(RuleCategoryID));
            if (InsertedBy <= 0)
                throw new ArgumentException("CreatedBy must be larger then zero", nameof(InsertedBy));

            switch (TypeID)
            {
                case RuleChangeType.Update:
                    if (string.IsNullOrWhiteSpace(RuleText) || RuleText.Length < 16)
                        throw new ArgumentException("Must be at least 16 characters", nameof(RuleText));
                    break;
                case RuleChangeType.Create:                    
                    if(string.IsNullOrWhiteSpace(RuleText) || RuleText.Length < 16)
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
        None,
        Update,
        Create,
        Delete
    }
}
