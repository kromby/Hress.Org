using Ez.Hress.Shared.Entities;

namespace Ez.Hress.Hardhead.Entities;

public class StatisticUserEntity : StatisticBase
{
    public StatisticUserEntity()
    {
        User = new UserBasicEntity();
    }

    public UserBasicEntity User { get; set; }
}