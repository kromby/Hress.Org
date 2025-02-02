using Ez.Hress.Shared.Entities;
using System.Collections.Generic;

namespace Ez.Hress.Hardhead.Entities;

public class Award : EntityBase<int>
{
    public HrefEntity Winners
    {
        get => new()
        {
            ID = this.ID,
            Href = string.Format("/api/hardhead/awards/{0}/winners", this.ID)
        };
    }

    public string? Href { get; set; }

    public IList<YearEntity> Years { get; set; } = new List<YearEntity>();
}

public class YearEntity : EntityBase<int>
{
    public int GuestCount { get; set; }
    public int? PhotoID { private get; set; }
    public HrefEntity Photo
    {
        get
        {
            if (PhotoID.HasValue)
                return new HrefEntity()
                {
                    Href = string.Format("/api/images/{0}/content", PhotoID.Value)
                };

            return null;
        }
    }

    public UserBasicEntity Hardhead { get; set; } = new();
}
