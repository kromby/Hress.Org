using Ez.Hress.Shared.Entities;

namespace Ez.Hress.MajorEvents.Entities;

public class PartyWinner : UserBasicEntity
{
    public PartyWinner(int id, string username, int count)
    {
        ID = id;
        Username = username;
        WinCount = count;
    }

    public int WinCount { get; set; }
}
