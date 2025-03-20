namespace Ez.Hress.Hardhead.Entities;

public class RatingInfo //: ICloneable
{
    public RatingInfo(string name, string code)
    {
        Name = name;
        Code = code;
    }

    public string Name { get; set; }
    public string Code { get; set; }
    public decimal? AverageRating { get; set; }
    public int NumberOfRatings { get; set; }
    public int? MyRating { get; set; }

    public object Clone()
    {
        return new RatingInfo(Name, Code)
        {
            AverageRating = AverageRating,
            NumberOfRatings = NumberOfRatings,
            MyRating = MyRating
        };
    }
}
