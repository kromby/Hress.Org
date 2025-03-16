using Ez.Hress.Shared.Entities;

namespace Ez.Hress.Hardhead.Entities;

public class RatingEntity
{
    public RatingEntity()
    {
        Ratings = new List<RatingInfo>();
    }

    public int ID { get; set; }

    public int? UserID { private get; set; }

    public HrefEntity? User
    {
        get
        {
            if (UserID.HasValue)
            {
                return new HrefEntity()
                {
                    ID = UserID,
                    Href = string.Format("/api/users/{0}", UserID)
                };
            }
            else
                return null;
        }
    }

    public IList<RatingInfo> Ratings
    {
        get; set;
    }
    public bool Readonly { get; set; }
}
