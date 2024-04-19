using Ez.Hress.Shared.Entities;

namespace Ez.Hress.UserProfile.Entities;

public class Relation : EntityBase<int>
{
    public Relation(int id, UserBasicEntity user, TypeEntity type)
    {
        ID = id;
        RelatedUser = user;
        Type = type;
    }

    public UserBasicEntity RelatedUser { get; set; }

    public TypeEntity Type { get; set; }
}
