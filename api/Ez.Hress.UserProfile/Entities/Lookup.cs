using Ez.Hress.Shared.Entities;

namespace Ez.Hress.UserProfile.Entities;

public class Lookup : EntityBase<int>
{
    public Lookup(int userId, int typeId, int valueId)
    {
        UserId = userId;
        TypeId = typeId;
        ValueId = valueId;
    }

    public int UserId { get; set; }
    public int TypeId { get; set; }
    public int ValueId { get; set; }
}

