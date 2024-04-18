using Ez.Hress.Shared.Entities;

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

    //public IList<YearEntity> Years { get; set; }
}
