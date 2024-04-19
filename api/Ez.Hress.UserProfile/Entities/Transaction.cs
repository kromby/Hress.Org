using Ez.Hress.Shared.Entities;

namespace Ez.Hress.UserProfile.Entities;

public class Transaction : EntityBase<int>
{
    public Transaction(int id, int amount, string name, UserBasicEntity user)
    {
        ID = id;
        Amount = amount;    
        Name = name;
        User = user;
    }

    public UserBasicEntity User { get; set; }
    public int Amount { get; set; }
}
