namespace Ez.Hress.Hardhead.Entities;

public class ChallangeHistory
{
    public ChallangeHistory()
    {
        Challengers = new List<StatisticUserEntity>();
        Challenged = new List<StatisticUserEntity>();
    }

    public ChallangeHistory(IList<StatisticUserEntity> challengers, IList<StatisticUserEntity> challenged)
    {
        Challengers = challengers;
        Challenged = challenged;
    }

    public IList<StatisticUserEntity> Challengers { get; set; }
    public IList<StatisticUserEntity> Challenged { get; set; }
}
